using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.SyncKnmiMetarFileList;

public static class Extensions
{
    public static void AddSyncKnmiMetarFileListFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISyncKnmiMetarFileListFeature, SyncKnmiMetarFileListFeature>();
        services.AddScoped<IMetarProcessor, MetarProcessor>();
        services.AddScoped<IMetarFileBulkRetriever, MetarFileBulkRetriever>();
        services.AddHttpClient<IKnmiMetarApiClient, KnmiMetarApiClient>();
        services.Configure<KnmiMetarApiOptions>(configuration.GetSection(nameof(KnmiMetarApiOptions)));
    }
}