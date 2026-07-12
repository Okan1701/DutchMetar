using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using DutchMetar.Core.Infrastructure.Accessors;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.DailyFileSync;

public class DailyFileSyncFeature : IDailyFileSyncFeature
{
    private readonly IKnmiRepository _knmiRepository;
    private readonly ILogger<DailyFileSyncFeature> _logger;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;
    private readonly INewKnmiFileHandler _newKnmiFileHandler;
    
    // Simple way to prevent rate limit; this controls the delay before the next file download.
    private const int FileDownloadIntervalMs = 1000;
    
    public DailyFileSyncFeature(ILogger<DailyFileSyncFeature> logger, IKnmiRepository knmiRepository, ICorrelationIdAccessor correlationIdAccessor, INewKnmiFileHandler newKnmiFileHandler)
    {
        _logger = logger;
        _knmiRepository = knmiRepository;
        _correlationIdAccessor = correlationIdAccessor;
        _newKnmiFileHandler = newKnmiFileHandler;
    }
    
    public async Task SyncKnmiMetarFiles(CancellationToken cancellationToken = default)
    {
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
                await _newKnmiFileHandler.HandleFileAsync(fileName, cancellationToken);
                await Task.Delay(FileDownloadIntervalMs, cancellationToken);
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