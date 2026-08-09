using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Metar.Notifications;

public class NewMetarKnmiNotificationFeature : INewMetarKnmiNotificationFeature
{
    private readonly IMetarFileHandler _metarFileHandler;

    public NewMetarKnmiNotificationFeature(IMetarFileHandler metarFileHandler)
    {
        _metarFileHandler = metarFileHandler;
    }

    public bool CanHandleMessage(FileEvent fileEvent)
    {
        return fileEvent?.Data?.DataSetName == KnmiDatasetNames.Metar;
    }

    public async Task HandleNotificationAsync(FileEvent fileEvent, CancellationToken cancellationToken)
    {
        var fileMeta = new KnmiFileMeta
        {
            FileName = fileEvent.Data?.FileName ?? string.Empty,
            CreatedOn = !string.IsNullOrEmpty(fileEvent.Time)
                ? DateTimeOffset.Parse(fileEvent.Time)
                : DateTimeOffset.MinValue
        };
        
        if (fileMeta.FileName == string.Empty || fileMeta.CreatedOn == DateTimeOffset.MinValue)
        {
            return;
        }
        
        await _metarFileHandler.HandleFileAsync(fileMeta, cancellationToken);
    }
}