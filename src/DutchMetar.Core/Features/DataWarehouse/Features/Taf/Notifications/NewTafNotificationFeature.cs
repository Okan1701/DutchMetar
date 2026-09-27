using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications.Contracts;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.Notifications;

public class NewTafNotificationFeature : INewTafNotificationFeature
{
    private readonly ILogger<NewTafNotificationFeature> _logger;
    private readonly ITafFileHandler _tafFileHandler;

    public NewTafNotificationFeature(ILogger<NewTafNotificationFeature> logger, ITafFileHandler tafFileHandler)
    {
        _logger = logger;
        _tafFileHandler = tafFileHandler;
    }

    public bool CanHandleMessage(FileEvent fileEvent)
    {
        return fileEvent.Data?.DataSetName == KnmiDatasetNames.Taf;
    }

    public async Task HandleNotificationAsync(FileEvent fileEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling new TAF notification: {FileName}", fileEvent.Data?.FileName);
        if (string.IsNullOrEmpty(fileEvent.Data?.FileName))
        {
            _logger.LogWarning("Received new TAF FileEvent with an empty FileName!");
            return;
        }
        
        await _tafFileHandler.HandleFileAsync(new KnmiFileMeta
        {
            FileName = fileEvent.Data.FileName,
            CreatedOn = !string.IsNullOrEmpty(fileEvent.Time)
                ? DateTimeOffset.Parse(fileEvent.Time)
                : DateTimeOffset.MinValue
        }, cancellationToken);
    }
}