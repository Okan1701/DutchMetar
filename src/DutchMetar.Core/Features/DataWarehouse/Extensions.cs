using DutchMetar.Core.Features.DataWarehouse.Features.DailyFileSync;
using DutchMetar.Core.Features.DataWarehouse.Features.Notifications;
using DutchMetar.Core.Features.DataWarehouse.Shared;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.Options;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.DataWarehouse;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public void AddDataWarehouseServices(IConfiguration configuration)
        {
            services.AddScoped<INotificationsFeature, NotificationsFeature>();
            services.AddScoped<IDailyFileSyncFeature, DailyFileSyncFeature>();
            services.AddScoped<INewKnmiFileHandler, NewKnmiFileHandler>();
            services.AddScoped<IMetarXmlMapper, MetarXmlMapper>();
            services.AddScoped<IKnmiRepository, KnmiRepository>();
            services.Configure<KnmiMetarApiOptions>(configuration.GetSection(nameof(KnmiMetarApiOptions)));
            services.AddHttpClient<IKnmiMetarApiClient, KnmiMetarApiClient>();
            services.AddSingleton<IKnmiNotificationClient, KnmiNotificationClient>();
        }
    }
}