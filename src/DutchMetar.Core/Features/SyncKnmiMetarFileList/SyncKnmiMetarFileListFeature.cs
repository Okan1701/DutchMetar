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
    
    // KNMI has 1000 requests per hour as rate limit
    private const int MaxRequests = 1000;
    private const int DefaultStartYear = 2025;
    
    public SyncKnmiMetarFileListFeature(IKnmiMetarApiClient knmiMetarApiClient, DutchMetarContext dutchMetarContext, ILogger<SyncKnmiMetarFileListFeature> logger)
    {
        _knmiMetarApiClient = knmiMetarApiClient;
        _dutchMetarContext = dutchMetarContext;
        _logger = logger;
    }

    public async Task SyncKnmiFiles(CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Starting KNMI Metar file sync. Correlation ID = {CorrelationId}", correlationId);
        var currentRequest = 0;
        var oldestSavedFile = await _dutchMetarContext.KnmiMetarFiles
            .OrderByDescending(x => x.FileCreatedAt)
            .LastOrDefaultAsync(cancellationToken);
        
        var startDate = new DateTime(DefaultStartYear, 1, 1);
        var endDate = oldestSavedFile?.FileCreatedAt;
        
        var currentSavedFileNames = await _dutchMetarContext.KnmiMetarFiles
            .Select(x => x.FileName)
            .ToArrayAsync(cancellationToken);
        
        // Initial request
        var parameters = new KnmiFilesParameters
        {
            Begin = startDate,
            End = endDate,
            Sorting = "desc",
            OrderBy = "created"
        };

        var isTruncated = true;
        while (isTruncated && !cancellationToken.IsCancellationRequested)
        {
            if (currentRequest > MaxRequests)
            {
                _logger.LogInformation("Max request quota reached. Task will be aborted. Correlation ID = {CorrelationId}", correlationId);
                return;
            }
            
            var data = await _knmiMetarApiClient.GetMetarFileSummaries(parameters, cancellationToken);

            if (data.Files.Count == 0)
            {
                break;
            }
            
            // Map response to entity
            var newEntities = data.Files
                .Where(x => currentSavedFileNames.All(y => y != x.Filename))
                .Select(x => x.ToKnmiMetarFileEntity())
                .Select(x =>
                {
                    x.CorrelationId = correlationId;
                    return x;
                });
            
            _dutchMetarContext.KnmiMetarFiles.AddRange(newEntities);
            await _dutchMetarContext.SaveChangesAsync(cancellationToken);

            isTruncated = data.IsTruncated;
            parameters.NextPageToken = data.NextPageToken;
            currentRequest++;
            
            _logger.LogDebug("Response batch processed. Quote: {CurrentQuota} / {MaxQuota}", currentRequest, MaxRequests);
        }

    }

}