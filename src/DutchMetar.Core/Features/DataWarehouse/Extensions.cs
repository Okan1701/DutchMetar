using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.Options;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DutchMetar.Core.Features.DataWarehouse;

public static class Extensions
{
    extension(IServiceCollection services)
    {
        public void AddDataWarehouseServices(IConfiguration configuration)
        {
            services.AddScoped<IDailyFileSyncFeature, DailyFileSyncFeature>();
            services.AddScoped<IRawMetarFileHandlingFeature, RawMetarFileHandlingFeature>();
            services.AddScoped<IMetarXmlMapper, MetarXmlMapper>();
            services.AddScoped<IKnmiRepository, KnmiRepository>();
            services.Configure<KnmiMetarApiOptions>(configuration.GetSection(nameof(KnmiMetarApiOptions)));
            services.AddHttpClient<IKnmiMetarApiClient, KnmiMetarApiClient>();
            services.AddSingleton<IKnmiNotificationClient, KnmiNotificationClient>();
        }
    }
}