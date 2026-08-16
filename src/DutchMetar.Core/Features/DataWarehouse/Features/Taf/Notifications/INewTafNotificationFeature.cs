using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.Notifications;

/// <summary>
/// Handles incoming KNMI TAF notifications.
/// </summary>
public interface INewTafNotificationFeature : IKnmiNotificationHandler
{
}