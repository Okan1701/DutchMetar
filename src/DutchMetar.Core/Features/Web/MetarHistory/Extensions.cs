using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.Web.MetarHistory;

public static class Extensions
{
    public static void AddMetarHistoryFeature(this IServiceCollection services)
    {
        services.AddScoped<IGetMetarHistoryFeature, GetMetarHistoryFeature>();
    }
}