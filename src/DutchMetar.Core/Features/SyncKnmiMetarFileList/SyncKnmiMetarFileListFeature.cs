using System.Text.RegularExpressions;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Exceptions;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.SyncKnmiMetarFileList;

public class SyncKnmiMetarFileListFeature : ISyncKnmiMetarFileListFeature
{
    private readonly IKnmiMetarApiClient _knmiMetarApiClient;
    private readonly DutchMetarContext _dutchMetarContext;
    private readonly ILogger<SyncKnmiMetarFileListFeature> _logger;
    private readonly IMetarProcessor _metarProcessor;
    
    // Will not retrieve files from older years
    private const int DefaultStartYear = 2025;
    private const string MetarStringRegex = @"<!--\s*(METAR.*?)\s*-->";
    private const string AirportNameRegex = @"<aixm:name>(.*?)<\/aixm:name>";

    
    public SyncKnmiMetarFileListFeature(IKnmiMetarApiClient knmiMetarApiClient, DutchMetarContext dutchMetarContext, ILogger<SyncKnmiMetarFileListFeature> logger, IMetarProcessor metarProcessor)
    {
        _knmiMetarApiClient = knmiMetarApiClient;
        _dutchMetarContext = dutchMetarContext;
        _logger = logger;
        _metarProcessor = metarProcessor;
    }
    
    public async Task SyncKnmiMetarFiles(CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Starting KNMI Metar file sync after oldest saved file. Correlation ID = {CorrelationId}", correlationId);
        var oldestSavedFileDate = await _dutchMetarContext.KnmiMetarFiles
            .OrderByDescending(x => x.FileCreatedAt)
            .Select(x => x.FileCreatedAt)
            .LastOrDefaultAsync(cancellationToken);
        
        var earliestSavedFileDate = await _dutchMetarContext.KnmiMetarFiles
            .OrderByDescending(x => x.FileCreatedAt)
            .Select(x => x.FileCreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        
        _logger.LogDebug("Current locally saved dataset time range: {OldestDate} - {EarliestDate}", oldestSavedFileDate, earliestSavedFileDate);
        
        // Parameters for retrieving the latest files
        var parametersToRetrieveNewFiles = new KnmiFilesParameters
        {
            Begin = earliestSavedFileDate,
            Sorting = "desc",
            OrderBy = "created"
        };
        
        // Parameters for retrieving files older than the currently saved files
        var parametersToRetrieveOlderFiles = new KnmiFilesParameters
        {
            Begin = oldestSavedFileDate,
            Sorting = "desc",
            OrderBy = "created"
        };

        try
        {
            await GetAndSaveKnmiFilesLoop(parametersToRetrieveNewFiles, cancellationToken, correlationId);
        }
        catch (MaxRequestLimitReachedException)
        {
            _logger.LogWarning("Aborting sync in progress: rate limit reached");
        }
    }

    public async Task SyncKnmiFilesAfterOldestSavedFile(CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Starting KNMI Metar file sync after oldest saved file. Correlation ID = {CorrelationId}", correlationId);
        var oldestSavedFile = await _dutchMetarContext.KnmiMetarFiles
            .OrderByDescending(x => x.FileCreatedAt)
            .LastOrDefaultAsync(cancellationToken);
        
        var startDate = new DateTime(DefaultStartYear, 1, 1);
        var endDate = oldestSavedFile?.FileCreatedAt;
        
        // Initial request, will be modified in the loop
        var parameters = new KnmiFilesParameters
        {
            Begin = startDate,
            End = endDate,
            Sorting = "desc",
            OrderBy = "created"
        };

        try
        {
            await GetAndSaveKnmiFilesLoop(parameters, cancellationToken, correlationId);
        }
        catch (MaxRequestLimitReachedException)
        {
            _logger.LogWarning("Aborting sync in progress: rate limit reached");
        }
    }
    
    public async Task SyncKnmiFilesBeforeEarliestSavedFile(CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Starting KNMI Metar file sync before earliest saved file. Correlation ID = {CorrelationId}", correlationId);
        var earliestSavedFileDateTime = await _dutchMetarContext.KnmiMetarFiles
            .OrderByDescending(x => x.FileCreatedAt)
            .Select(x => x.FileCreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (earliestSavedFileDateTime == default)
        {
            // If database is still empty, then let SyncKnmiFilesAfterOldestSavedFile do the initial syncing
            // to prevent possible conflicts
            _logger.LogInformation("Aborting sync: database is currently empty");
            return;
        }
        
        // Initial request
        var parameters = new KnmiFilesParameters
        {
            Begin = earliestSavedFileDateTime,
            Sorting = "asc",
            OrderBy = "created"
        };

        try
        {
            await GetAndSaveKnmiFilesLoop(parameters, cancellationToken, correlationId);
        }
        catch (MaxRequestLimitReachedException)
        {
            _logger.LogWarning("Aborting sync in progress: rate limit reached");
        }
    }

    private async Task GetAndSaveKnmiFilesLoop(KnmiFilesParameters parameters, CancellationToken cancellationToken, Guid correlationId)
    {
        var isTruncated = true;
        
        var currentSavedFileNames = await _dutchMetarContext.KnmiMetarFiles
            .Select(x => x.FileName)
            .ToArrayAsync(cancellationToken);
        
        while (isTruncated && !cancellationToken.IsCancellationRequested)
        {
            var data = await _knmiMetarApiClient.GetMetarFileSummaries(parameters, cancellationToken);

            if (data.Files.Count == 0)
            {
                break;
            }

            var filteredFiles = data.Files
                .Where(x => currentSavedFileNames.All(y => y != x.Filename))
                .OrderBy(x => x.Created);
            
            // For each file, convert it into an entity and process the raw metar
            foreach (var file in filteredFiles)
            {
                var fileContent = await _knmiMetarApiClient.GetKnmiMetarFileContentAsync(file.Filename, cancellationToken);
                var entity = file.ToKnmiMetarFileEntity();
                
                var metarStringMatch = Regex.Match(fileContent, MetarStringRegex);
                var nameStringMatch = Regex.Match(fileContent, AirportNameRegex);

                if (metarStringMatch.Success)
                {
                    var airportName = nameStringMatch.Success ? nameStringMatch.Value : null;
                    try
                    {
                        await _metarProcessor.ProcessRawMetarAsync(metarStringMatch.Value, airportName, cancellationToken);
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
            
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Aborting KNMI metar sync, cancellation was requested!");
                break;
            }
        }
    }
}