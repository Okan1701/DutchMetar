using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.DailySync;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Infrastructure.Accessors;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Metar.DailySync;

public class DailyMetarSyncFeature : IDailyMetarSyncFeature
{
    private readonly IKnmiRepository _knmiRepository;
    private readonly ILogger<DailyMetarSyncFeature> _logger;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;
    private readonly IMetarFileHandler _metarFileHandler;
    
    // Simple way to prevent rate limit; this controls the delay before the next file download.
    private const int FileDownloadIntervalMs = 1000;
    private const int MaxRequests = 1000;
    
    public DailyMetarSyncFeature(ILogger<DailyMetarSyncFeature> logger, IKnmiRepository knmiRepository, ICorrelationIdAccessor correlationIdAccessor, IMetarFileHandler metarFileHandler)
    {
        _logger = logger;
        _knmiRepository = knmiRepository;
        _correlationIdAccessor = correlationIdAccessor;
        _metarFileHandler = metarFileHandler;
    }
    
    public async Task SyncKnmiMetarFiles(CancellationToken cancellationToken = default)
    {
        var requestCounter = 1;
        var scope = _logger.BeginScope(new KeyValuePair<string, object?>[]
        {
            new("CorrelationId", _correlationIdAccessor.CorrelationId),
            new("SyncStartDateTimeUtc", DateTime.UtcNow),
        });
        _logger.LogInformation("Starting KNMI Metar file sync.");
        
        // Parameters for retrieving files from the last 24h.
        // The end parameter is set 1 hour behind current time to prevent overlap with MQTT messages.
        var end = DateTimeOffset.UtcNow.AddHours(-1);
        var parameters = new KnmiFilesParameters
        {
            End = end,
            Begin = end.AddDays(-1),
            Sorting = "desc",
            OrderBy = "created",
            MaxKeys = 1000,
        };
        
        try
        {
            _logger.LogInformation("Retrieving KNMI metar fails over the last 24 hours.");
            var fileNames = await _knmiRepository.GetKnmiMetarFiles(parameters, cancellationToken,
                _correlationIdAccessor.CorrelationId);

            foreach (var fileName in fileNames)
            {
                // Safety guard on our end
                if (requestCounter > MaxRequests) throw new KnmiRateLimitReachedException();
                
                await _metarFileHandler.HandleFileAsync(fileName, cancellationToken);
                await Task.Delay(FileDownloadIntervalMs, cancellationToken);
                requestCounter++;
            }
            
            
        }
        catch (KnmiRateLimitReachedException)
        {
            _logger.LogWarning("Aborting sync: rate limit reached");
        }
        catch (KnmiApiException ex)
        {
            _logger.LogError(ex, "Aborting sync: the following {StatusCode} API error occured: {ApiError}", ex.StatusCode, ex.Message);
        }   
        
        _logger.LogInformation("Finished daily KNMI Metar file sync.");
        scope?.Dispose();
    }
}