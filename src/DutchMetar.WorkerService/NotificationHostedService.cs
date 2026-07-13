using DutchMetar.Core.Features.DataWarehouse.Features.Notifications;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiNotifications;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;

namespace DutchMetar.WorkerService;

public class NotificationHostedService : BackgroundService
{
    private readonly IKnmiNotificationClient _knmiNotificationClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NotificationHostedService(IKnmiNotificationClient knmiNotificationClient, IServiceScopeFactory serviceScopeFactory)
    {
        _knmiNotificationClient = knmiNotificationClient;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ReadAllAsync() keeps going until the Channel is completed.
        // Since the client never completes the channel, this loop will exist until app is stopped :D
        await foreach (var fileEvent in _knmiNotificationClient
                           .ChannelReader
                           .ReadAllAsync(stoppingToken))
        {
            var scope = _serviceScopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<INotificationsFeature>();
            
            var fileMeta = new KnmiFileMeta
            {
                FileName = fileEvent.Data?.FileName ?? string.Empty,
                CreatedOn = !string.IsNullOrEmpty(fileEvent.Time)
                    ? DateTimeOffset.Parse(fileEvent.Time)
                    : DateTimeOffset.MinValue
            };
        
            await handler.HandleFileAsync(fileMeta, stoppingToken);
            scope.Dispose();
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _knmiNotificationClient.ConnectAndReceiveAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _knmiNotificationClient.DisconnectAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}