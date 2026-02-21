using DutchMetar.Core.Features.AirportPerDayHistory.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.AirportPerDayHistory;

public static class Extensions
{
    public static void AddAirportDayHistoryFeature(this IServiceCollection services)
    {
        services.AddScoped<IGetAirportDayHistoryFeature, GetAirportDayHistoryFeature>();
    }
}