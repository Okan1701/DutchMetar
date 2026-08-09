using DutchMetar.Core.Features.DataWarehouse.Features.Metar.DailySync;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Notifications;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Parsers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.Options;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.DataWarehouse;

public static class Extensions
{
    public static void AddDataWarehouseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INewMetarKnmiNotificationFeature, NewMetarKnmiNotificationFeature>();
        services.AddScoped<IKnmiNotificationHandler>(provider => provider.GetRequiredService<INewMetarKnmiNotificationFeature>());
        services.AddScoped<IDailyMetarSyncFeature, DailyMetarSyncFeature>();
        services.AddScoped<IMetarFileHandler, MetarFileHandler>();
        services.AddScoped<IMetarXmlParser, MetarXmlParser>();
        services.AddScoped<IKnmiRepository, KnmiRepository>();
        services.Configure<KnmiDataSourceOptions>(configuration.GetSection(nameof(KnmiDataSourceOptions)));
        services.AddHttpClient<IKnmiApiClient, KnmiApiClient>();
        services.AddSingleton<IKnmiNotificationClient, KnmiNotificationClient>();
    }
}