using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Notifications;

public class NotificationsFeature : INotificationsFeature
{
    private readonly INewKnmiFileHandler _newKnmiFileHandler;

    public NotificationsFeature(INewKnmiFileHandler newKnmiFileHandler)
    {
        _newKnmiFileHandler = newKnmiFileHandler;
    }

    public async Task HandleFileAsync(KnmiFileMeta fileMeta, CancellationToken cancellationToken)
    {
        await  _newKnmiFileHandler.HandleFileAsync(fileMeta, cancellationToken);
    }
}