using DutchMetar.Core.Features.Web.AirportSummary.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.Web.AirportSummary;

public static class Extensions
{
    public static void AddAirportSummaryFeature(this IServiceCollection services)
    {
        services.AddScoped<IGetAirportSummariesFeature, GetAirportSummariesFeature>();
    }
}