using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Interfaces;

namespace DutchMetar.Hangfire.Host;

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
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IKnmiFileEventHandler>();
        var fileEvents = await _knmiNotificationClient
            .ChannelReader
            .ReadAllAsync(stoppingToken)
            .ToArrayAsync(stoppingToken);
        
        await handler.HandleFileEventsAsync(fileEvents);
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