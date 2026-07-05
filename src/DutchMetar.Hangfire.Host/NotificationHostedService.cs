using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Interfaces;

namespace DutchMetar.Hangfire.Host;

public class NotificationHostedService : BackgroundService
{
    private readonly IKnmiNotificationClient _knmiNotificationClient;

    public NotificationHostedService(IKnmiNotificationClient knmiNotificationClient)
    {
        _knmiNotificationClient = knmiNotificationClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (string item in _knmiNotificationClient.ChannelReader.ReadAllAsync(stoppingToken))
        {
            Console.WriteLine($"Received: {item}");
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