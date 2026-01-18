using DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure;
using DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure.Contracts;
using DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure.Exceptions;
using DutchMetar.Core.Features.SyncKnmiMetar.Interfaces;
using DutchMetar.Core.Infrastructure.Accessors;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.SyncKnmiMetar;

public class SyncKnmiMetarFileListFeature : ISyncKnmiMetarFileListFeature
{
    private readonly DutchMetarContext _dutchMetarContext;
    private readonly IMetarFileBulkRetriever _fileBulkRetriever;
    private readonly ILogger<SyncKnmiMetarFileListFeature> _logger;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;
    
    // Will not retrieve files from older years
    private const int DefaultStartYear = 2023;
    
    public SyncKnmiMetarFileListFeature(DutchMetarContext dutchMetarContext, ILogger<SyncKnmiMetarFileListFeature> logger, IMetarFileBulkRetriever fileBulkRetriever, ICorrelationIdAccessor correlationIdAccessor)
    {
        _dutchMetarContext = dutchMetarContext;
        _logger = logger;
        _fileBulkRetriever = fileBulkRetriever;
        _correlationIdAccessor = correlationIdAccessor;
    }
    
    public async Task SyncKnmiMetarFiles(CancellationToken cancellationToken = default)
    {
        var scope = _logger.BeginScope(new KeyValuePair<string, object?>[]
        {
            new("CorrelationId", _correlationIdAccessor.CorrelationId),
            new("SyncStartDateTimeUtc", DateTime.UtcNow),
        });
        _logger.LogInformation("Starting KNMI Metar file sync.");
        
        var hasAnyFiles = await _dutchMetarContext.KnmiMetarFiles.AnyAsync(cancellationToken);
        var oldestSavedFileDate = await _dutchMetarContext.KnmiMetarFiles
            .OrderByDescending(x => x.FileCreatedAt)
            .Select(x => x.FileCreatedAt)
            .LastOrDefaultAsync(cancellationToken);
        
        var newestSavedFile = await _dutchMetarContext.KnmiMetarFiles
            .OrderByDescending(x => x.FileCreatedAt)
            .Select(x => x.FileCreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        
        _logger.LogDebug("Current locally saved dataset time range: {OldestDate} - {EarliestDate}", oldestSavedFileDate, newestSavedFile);
        
        // Parameters for retrieving the latest files
        var parametersToRetrieveNewFiles = new KnmiFilesParameters
        {
            Begin = newestSavedFile != default ? newestSavedFile : null,
            Sorting = "desc",
            OrderBy = "created"
        };
        
        // Parameters for retrieving files older than the currently saved files
        var parametersToRetrieveOlderFiles = new KnmiFilesParameters
        {
            End = oldestSavedFileDate,
            Begin = new DateTime(DefaultStartYear, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Sorting = "desc",
            OrderBy = "created"
        };

        try
        {
            _logger.LogInformation("Retrieving and processing new KNMI metar files");
            await _fileBulkRetriever.GetAndSaveKnmiFiles(parametersToRetrieveNewFiles, cancellationToken,
                _correlationIdAccessor.CorrelationId);
            if (hasAnyFiles)
            {
                _logger.LogInformation("Retrieving and processing older KNMI files");
                await _fileBulkRetriever.GetAndSaveKnmiFiles(parametersToRetrieveOlderFiles, cancellationToken,
                    _correlationIdAccessor.CorrelationId);
            }
        }
        catch (KnmiRateLimitReachedException)
        {
            _logger.LogWarning("Aborting sync in progress: rate limit reached");
        }
        catch (KnmiApiException ex)
        {
            _logger.LogError(ex, "Aborting sync: the following {StatusCode} API error occured: {ApiError}", ex.StatusCode, ex.Message);
        }
        
        scope?.Dispose();
    }
}