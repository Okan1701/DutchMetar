using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Metar.Notifications;

/// <summary>
/// Feature for handling new notifications received from KNMI Notifications service.
/// </summary>
public interface INewMetarKnmiNotificationFeature : IKnmiNotificationHandler
{
}