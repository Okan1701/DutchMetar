using DutchMetar.Core.Features.LoadDutchMetars;
using DutchMetar.Core.Features.LoadDutchMetars.Interfaces;
using DutchMetar.Core.Features.ProcessKnmiMetarFiles.Infrastructure;
using DutchMetar.Core.Features.ProcessKnmiMetarFiles.Infrastructure.Interfaces;
using DutchMetar.Core.Features.ProcessKnmiMetarFiles.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.ProcessKnmiMetarFiles;

public static class Extensions
{
    public static void AddProcessKnmiMetarFilesFeature(this IServiceCollection services)
    {
        services.AddScoped<IProcessKnmiMetarFilesFeature, ProcessKnmiMetarFilesFeature>();
        services.AddScoped<IKnmiMetarFileClient, KnmiMetarFileClient>();
        services.AddScoped<IMetarMapper, MetarMapper>();
        services.AddHttpClient<IKnmiMetarRepository, KnmiMetarRepository>();
    }
}