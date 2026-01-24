using System.Text.RegularExpressions;
using DutchMetar.Core.Features.SyncKnmiMetar.Exceptions;
using DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure.Contracts;
using DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetar.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.SyncKnmiMetar.Services;

public class MetarFileBulkRetriever : IMetarFileBulkRetriever
{
    private readonly IKnmiMetarApiClient _knmiMetarApiClient;
    private readonly DutchMetarContext _dutchMetarContext;
    private readonly ILogger<MetarFileBulkRetriever> _logger;
    private readonly IMetarProcessor _metarProcessor;

    public MetarFileBulkRetriever(IKnmiMetarApiClient knmiMetarApiClient, DutchMetarContext dutchMetarContext, ILogger<MetarFileBulkRetriever> logger, IMetarProcessor metarProcessor)
    {
        _knmiMetarApiClient = knmiMetarApiClient;
        _dutchMetarContext = dutchMetarContext;
        _logger = logger;
        _metarProcessor = metarProcessor;
    }
    
    private const string MetarStringRegex = @"<!--(\s*(?:METAR|SPECI).*?\s*)-->";
    private const string AirportNameRegex = @"<aixm:name>(.*?)<\/aixm:name>";
    
    public async Task GetAndSaveKnmiFiles(KnmiFilesParameters parameters, CancellationToken cancellationToken, Guid correlationId)
    {
        var isTruncated = true;
        
        var currentSavedFileNames = await _dutchMetarContext.KnmiMetarFiles
            .Select(x => x.FileName)
            .ToArrayAsync(cancellationToken);
        
        _logger.LogDebug("There are already {SavedFilesCount} existing saved files", currentSavedFileNames.Length);
        
        while (isTruncated && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogTrace("Retrieving next batch of metar files");
            var data = await _knmiMetarApiClient.GetMetarFileSummaries(parameters, cancellationToken);

            if (data.Files.Count == 0)
            {
                _logger.LogTrace("Empty file array returned from API. Aborting loop.");
                break;
            }

            var filteredFiles = data.Files
                .Where(x => currentSavedFileNames.All(y => y != x.Filename))
                .OrderBy(x => x.Created);
            
            // For each file, convert it into an entity and process the raw metar
            foreach (var file in filteredFiles)
            {
                _logger.LogDebug("Retrieving file {FileName}.", file.Filename);
                var fileContent = await _knmiMetarApiClient.GetKnmiMetarFileContentAsync(file.Filename, cancellationToken);
                var entity = file.ToKnmiMetarFileEntity();
                
                var metarStringMatch = Regex.Match(fileContent, MetarStringRegex);
                var nameStringMatch = Regex.Match(fileContent, AirportNameRegex);

                if (metarStringMatch.Success)
                {
                    var airportName = nameStringMatch.Success ? nameStringMatch.Groups[1].Value : null;
                    entity.ExtractedRawMetar = metarStringMatch.Groups[1].Value.Trim();
                    try
                    {
                        await _metarProcessor.ProcessRawMetarAsync(entity.ExtractedRawMetar, airportName, cancellationToken);
                        entity.IsFileProcessed = true;
                    }
                    catch (MetarParseException ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        entity.IsFileProcessed = false;
                    }
                }
                else
                {
                    _logger.LogError("Failed to extract raw METAR from {FileName}", file.Filename);
                    entity.IsFileProcessed = false;
                }
                
                _dutchMetarContext.KnmiMetarFiles.Add(entity);
                await _dutchMetarContext.SaveChangesAsync(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Aborting KNMI metar sync, cancellation was requested!");
                    break;
                }
            }

            isTruncated = data.IsTruncated;
            parameters.NextPageToken = data.NextPageToken;

            if (string.IsNullOrEmpty(data.NextPageToken)) break;
            
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Aborting KNMI metar sync, cancellation was requested!");
                break;
            }
        }
    }
}