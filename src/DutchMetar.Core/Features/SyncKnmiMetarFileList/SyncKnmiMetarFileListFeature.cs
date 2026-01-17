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
    private readonly DutchMetarContext _dutchMetarContext;
    private readonly IMetarFileBulkRetriever _fileBulkRetriever;
    private readonly ILogger<SyncKnmiMetarFileListFeature> _logger;
    
    // Will not retrieve files from older years
    private const int DefaultStartYear = 2025;
    
    public SyncKnmiMetarFileListFeature(DutchMetarContext dutchMetarContext, ILogger<SyncKnmiMetarFileListFeature> logger, IMetarFileBulkRetriever fileBulkRetriever)
    {
        _dutchMetarContext = dutchMetarContext;
        _logger = logger;
        _fileBulkRetriever = fileBulkRetriever;
    }
    
    public async Task SyncKnmiMetarFiles(CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Starting KNMI Metar file sync after oldest saved file. Correlation ID = {CorrelationId}", correlationId);
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
            await _fileBulkRetriever.GetAndSaveKnmiFiles(parametersToRetrieveNewFiles, cancellationToken, correlationId);
            if (hasAnyFiles)
            {
                await _fileBulkRetriever.GetAndSaveKnmiFiles(parametersToRetrieveOlderFiles, cancellationToken, correlationId);
            }
        }
        catch (MaxRequestLimitReachedException)
        {
            _logger.LogWarning("Aborting sync in progress: rate limit reached");
        }
    }
}