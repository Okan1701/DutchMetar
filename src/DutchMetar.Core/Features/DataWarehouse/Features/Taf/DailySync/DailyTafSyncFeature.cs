using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.DailySync;

public class DailyTafSyncFeature : IDailyTafSyncFeature
{
    private readonly IKnmiRepository _knmiRepository;
    private readonly ILogger<DailyTafSyncFeature> _logger;
    private readonly ITafFileHandler _tafFileHandler;

    private const int FileDownloadIntervalMs = 1000;
    private const int MaxRequests = 1000;

    public DailyTafSyncFeature(
        ILogger<DailyTafSyncFeature> logger,
        IKnmiRepository knmiRepository,
        ITafFileHandler tafFileHandler)
    {
        _logger = logger;
        _knmiRepository = knmiRepository;
        _tafFileHandler = tafFileHandler;
    }

    public async Task SyncKnmiTafFiles(CancellationToken cancellationToken = default)
    {
        var requestCounter = 1;
        using var scope = _logger.BeginScope(new KeyValuePair<string, object?>[]
        {
            new("SyncStartDateTimeUtc", DateTime.UtcNow),
        });
        _logger.LogInformation("Starting KNMI TAF file sync.");

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
            _logger.LogInformation("Retrieving KNMI TAF files over the last 24 hours.");
            var fileMetas = await _knmiRepository.GetKnmiTafFiles(parameters, cancellationToken);

            foreach (var fileMeta in fileMetas)
            {
                if (requestCounter > MaxRequests)
                    throw new KnmiRateLimitReachedException();

                await _tafFileHandler.HandleFileAsync(fileMeta, cancellationToken);
                await Task.Delay(FileDownloadIntervalMs, cancellationToken);
                requestCounter++;
            }
        }
        catch (KnmiRateLimitReachedException)
        {
            _logger.LogWarning("Aborting TAF sync: rate limit reached");
        }
        catch (KnmiApiException ex)
        {
            _logger.LogError(ex, "Aborting TAF sync: the following {StatusCode} API error occurred: {ApiError}",
                ex.StatusCode, ex.Message);
        }

        _logger.LogInformation("Finished daily KNMI TAF file sync.");
    }
}
