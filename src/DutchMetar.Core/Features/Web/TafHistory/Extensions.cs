using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.Web.TafHistory;

public static class Extensions
{
    public static void AddTafHistoryFeature(this IServiceCollection services)
    {
        services.AddScoped<IGetTafHistoryFeature, GetTafHistoryFeature>();
    }
}
