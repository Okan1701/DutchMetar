using DutchMetar.Core.Features.LoadDutchMetars.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.LoadDutchMetars;

public static class Extensions
{
    public static void AddLoadDutchMetarsFeature(this IServiceCollection services)
    {
        services.AddScoped<ILoadDutchMetarsFeature, LoadDutchMetarsFeature>();
        services.AddScoped<IKnmiMetarRepository, KnmiMetarRepository>();
        services.AddScoped<IMetarMapper, MetarMapper>();
        services.AddHttpClient<IKnmiMetarRepository, KnmiMetarRepository>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
        });
    }
}