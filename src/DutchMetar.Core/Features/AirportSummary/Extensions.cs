using DutchMetar.Core.Features.AirportSummary.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.AirportSummary;

public static class Extensions
{
    public static void AddAirportSummaryFeature(this IServiceCollection services)
    {
        services.AddScoped<IGetAirportSummariesFeature, GetAirportSummariesFeature>();
    }
}