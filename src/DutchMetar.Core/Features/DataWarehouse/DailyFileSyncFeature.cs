using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.Contracts.DataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Interfaces;
using DutchMetar.Core.Infrastructure.Accessors;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse;

public class DailyFileSyncFeature : IDailyFileSyncFeature
{
    private readonly IKnmiRepository _knmiRepository;
    private readonly ILogger<DailyFileSyncFeature> _logger;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;
    private readonly IRawMetarFileHandlingFeature _rawMetarFileHandler;
    
    public DailyFileSyncFeature(ILogger<DailyFileSyncFeature> logger, IKnmiRepository knmiRepository, ICorrelationIdAccessor correlationIdAccessor, IRawMetarFileHandlingFeature rawMetarFileHandler)
    {
        _logger = logger;
        _knmiRepository = knmiRepository;
        _correlationIdAccessor = correlationIdAccessor;
        _rawMetarFileHandler = rawMetarFileHandler;
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
        var parameters = new KnmiFilesParameters
        {
            Begin = DateTimeOffset.UtcNow.AddDays(-1),
            Sorting = "desc",
            OrderBy = "created",
            MaxKeys = 1000,
        };
        
        try
        {
            _logger.LogInformation("Retrieving KNMI metar fails over the last 24 hours.");
            var fileNames = await _knmiRepository.GetKnmiMetarFiles(parameters, cancellationToken,
                _correlationIdAccessor.CorrelationId);
            
            await _rawMetarFileHandler.HandleFilesAsync(fileNames);
            
            
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