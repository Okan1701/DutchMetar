using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications.Contracts;

namespace DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications;

/// <summary>
/// Interface for handling incoming notifications from KNMI Notification Service
/// Classes that implement this interface will be called if the handler matches the message tyype
/// </summary>
public interface IKnmiNotificationHandler
{
    /// <summary>
    /// Method that checks if the incoming message can be handled by this handler
    /// If this method returns true, then it will be handled by this.
    /// </summary>
    bool CanHandleMessage(FileEvent fileEvent);

    /// <summary>
    /// Method for handling the incoming message.
    /// This method will be called when the message topic matches this handler.
    /// </summary>
    /// <param name="fileEvent">Received message</param>
    /// <param name="cancellationToken">Optional token used to cancel the handling</param>
    Task HandleNotificationAsync(FileEvent fileEvent, CancellationToken cancellationToken = default);
}