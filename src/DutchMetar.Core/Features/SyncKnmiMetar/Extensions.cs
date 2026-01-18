using DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure;
using DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetar.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetar.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.SyncKnmiMetar;

public static class Extensions
{
    public static void AddSyncKnmiMetarFileListFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISyncKnmiMetarFileListFeature, SyncKnmiMetarFileListFeature>();
        services.AddScoped<IMetarMapper, MetarMapper>();
        services.AddScoped<IMetarProcessor, MetarProcessor>();
        services.AddScoped<IMetarFileBulkRetriever, MetarFileBulkRetriever>();
        services.AddHttpClient<IKnmiMetarApiClient, KnmiMetarApiClient>();
        services.Configure<KnmiMetarApiOptions>(configuration.GetSection(nameof(KnmiMetarApiOptions)));
    }
}