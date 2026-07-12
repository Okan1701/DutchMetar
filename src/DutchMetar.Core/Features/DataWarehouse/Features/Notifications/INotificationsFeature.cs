using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Notifications;

/// <summary>
/// Feature for handling new notifications received from KNMI Notifications service.
/// </summary>
public interface INotificationsFeature
{
    /// <summary>
    /// Handle a file that was received as a notification.
    /// </summary>
    /// <param name="fileMeta">Meta data of the file.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandleFileAsync(KnmiFileMeta fileMeta, CancellationToken cancellationToken);
}