using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Interfaces;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IRawMetarFileHandlingFeature>();
            var fileEvents = await _knmiNotificationClient
                .ChannelReader
                .ReadAllAsync(stoppingToken)
                .ToArrayAsync(stoppingToken);

            var mappedFiles = fileEvents
                .Where(x => !string.IsNullOrEmpty(x.Time) && !string.IsNullOrEmpty(x.Data?.FileName))
                .Select(x => new KnmiFileMeta
                {
                    FileName = x.Data?.FileName ?? string.Empty,
                    CreatedOn = !string.IsNullOrEmpty(x.Time) ? DateTimeOffset.Parse(x.Time) : DateTimeOffset.MinValue,
                }).ToArray();
        
            await handler.HandleFilesAsync(mappedFiles);
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