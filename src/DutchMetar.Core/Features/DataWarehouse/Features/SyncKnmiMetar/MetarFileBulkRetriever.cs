using System.Text.RegularExpressions;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar;

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
    
    // These regex are used to extract the relevant info from the file XML content.
    private const string MetarStringRegex = @"<!--(\s*(?:METAR|SPECI).*?\s*)-->";
    private const string AirportNameRegex = @"<aixm:name>(.*?)<\/aixm:name>";
    
    public async Task GetAndSaveKnmiFiles(KnmiFilesParameters parameters, CancellationToken cancellationToken, Guid correlationId)
    {
        // Truncated means that the last request is not the final page.
        // So we can keep retrieving the next page.
        var isTruncated = true;
        
        // Retrieve current saved filenames in order to avoid inserting duplicate files.
        var currentSavedFileNames = await _dutchMetarContext.KnmiMetarFiles
            .Select(x => x.FileName)
            .ToArrayAsync(cancellationToken);
        
        _logger.LogDebug("There are already {SavedFilesCount} existing saved files", currentSavedFileNames.Length);
        
        // Main loop of the bulk retrieval process.
        // This will in essence retrieve a list of files, retrieve content of each file and process it into Airport and Metar entities.
        // This loop will continue until the API response indicates that all files have been listed.
        while (isTruncated && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogTrace("Retrieving next batch of metar files");
            
            // Get list of available METAR files.
            var data = await _knmiMetarApiClient.GetMetarFileSummaries(parameters, cancellationToken);
            
            // If the API returns empty array, then we most likely reached the end.
            if (data.Files.Count == 0)
            {
                _logger.LogTrace("Empty file array returned from API. Aborting loop.");
                break;
            }
            
            // Filter out filenames that already exist in the database.
            var filteredFiles = data.Files
                .Where(x => currentSavedFileNames.All(y => y != x.Filename))
                .OrderBy(x => x.Created);
            
            // For each file, retrieve the contents and convert it into domain entities.
            foreach (var file in filteredFiles)
            {
                _logger.LogDebug("Retrieving file {FileName}.", file.Filename);
                
                // Retrieve the string contents of the file.
                var fileContent = await _knmiMetarApiClient.GetKnmiMetarFileContentAsync(file.Filename, cancellationToken);
                
                // Convert the file API summary result into a domain entity.
                // Additional properties of the entity will be set below.
                var entity = file.ToKnmiMetarFileEntity();
                
                var metarStringMatch = Regex.Match(fileContent, MetarStringRegex);
                var nameStringMatch = Regex.Match(fileContent, AirportNameRegex);

                if (metarStringMatch.Success)
                {
                    // Retrieving the airport full name is nice to have, but not required.
                    var airportName = nameStringMatch.Success ? nameStringMatch.Groups[1].Value : null;
                    
                    entity.ExtractedRawMetar = metarStringMatch.Groups[1].Value.Trim();
                    try
                    {
                        // Process the raw METAR into domain entities.
                        await _metarProcessor.ProcessRawMetarAsync(entity.ExtractedRawMetar, airportName, file.Created, cancellationToken);
                        entity.IsFileProcessed = true;
                    }
                    catch (MetarParseException ex)
                    {
                        // If the METAR parsing goes wrong,
                        // then set IsFileProcessed to false so that it can be checked again at a later date.
                        _logger.LogError(ex, ex.Message);
                        entity.IsFileProcessed = false;
                    }
                }
                else
                {
                    // Failed to extract the raw METAR string from the file.
                    // File summary will still be kept in the database for a future checkup.
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
            
            // This controls if the main loop continues.
            isTruncated = data.IsTruncated;
            
            // API result contains special token required to fetch the next page.
            parameters.NextPageToken = data.NextPageToken;
            
            // If this token is empty, then no next page is available, so we can stop.
            if (string.IsNullOrEmpty(data.NextPageToken)) break;
            
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Aborting KNMI metar sync, cancellation was requested!");
                break;
            }
        }
    }
}