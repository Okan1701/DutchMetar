using DutchMetar.Core.Features.DataWarehouse.Features.ReprocessFailedFiles;
using DutchMetar.Core.Features.DataWarehouse.Features.ReprocessFailedFiles.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Interfaces;
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
        public void AddSyncKnmiMetarFileListFeature(IConfiguration configuration)
        {
            services.TryAddSharedServices();
            services.AddScoped<ISyncKnmiMetarFileListFeature, SyncKnmiMetarFileListFeature>();
            services.AddScoped<IMetarFileBulkRetriever, MetarFileBulkRetriever>();
            services.AddHttpClient<IKnmiMetarApiClient, KnmiMetarApiClient>();
            services.Configure<KnmiMetarApiOptions>(configuration.GetSection(nameof(KnmiMetarApiOptions)));
        }

        public void AddReprocessFailedFilesFeature()
        {
            services.TryAddSharedServices();
            services.AddScoped<IReprocessFailedFilesFeature, ReprocessFailedFilesFeature>();
        }

        private void TryAddSharedServices()
        {
            services.TryAddScoped<IMetarMapper, MetarMapper>();
            services.TryAddScoped<IMetarProcessor, MetarProcessor>();
        }
    }
}