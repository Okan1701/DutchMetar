using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Infrastructure.HostedServices;

public class NotificationHostedService : BackgroundService
{
    private readonly IKnmiNotificationClient _knmiNotificationClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<NotificationHostedService> _logger;

    public NotificationHostedService(IKnmiNotificationClient knmiNotificationClient, IServiceScopeFactory serviceScopeFactory, ILogger<NotificationHostedService> logger)
    {
        _knmiNotificationClient = knmiNotificationClient;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var fileEvent in _knmiNotificationClient
                           .ChannelReader
                           .ReadAllAsync(stoppingToken))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IKnmiNotificationHandler>().ToArray();
            
            var isHandled = false;
            foreach (var handler in handlers)
            {
                if (!handler.CanHandleMessage(fileEvent)) continue;
                await handler.HandleNotificationAsync(fileEvent, stoppingToken);
                isHandled = true;
            }
            if (!isHandled)
            {
                _logger.LogWarning("An incoming message could not handled! Id = {Id} Type = {Type}, Dataset = {DataSetName}", 
                    fileEvent.Id, 
                    fileEvent.Type, 
                    fileEvent.Data?.DataSetName);
            }
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