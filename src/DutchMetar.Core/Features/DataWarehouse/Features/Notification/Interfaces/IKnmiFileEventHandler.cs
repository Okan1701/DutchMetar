using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Contracts;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Notification.Interfaces;

public interface IKnmiFileEventHandler
{
    Task HandleFileEventsAsync(ICollection<FileEvent> fileEvents);
}