using System.Threading.Channels;
using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Features.Notification.EventArgs;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Notification.Interfaces;

/// <summary>
/// Represents a client for connecting and receiving messages from the <seealso href="https://developer.dataplatform.knmi.nl/notification-service">KNMI Notification Service</seealso>.
/// </summary>
public interface IKnmiNotificationClient : IDisposable
{
    /// <summary>
    /// Connect to the notification service and begin listening for messages.
    /// </summary>
    /// <returns></returns>
    Task ConnectAndReceiveAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Disconnect from the notification service.
    /// </summary>
    /// <returns></returns>
    Task DisconnectAsync(CancellationToken cancellationToken);

    // Channel Reader for obtaining the incoming messages in an async manner.
    ChannelReader<FileEvent> ChannelReader { get; }
}