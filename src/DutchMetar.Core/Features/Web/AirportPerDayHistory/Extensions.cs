using DutchMetar.Core.Features.Web.AirportPerDayHistory.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.Web.AirportPerDayHistory;

public static class Extensions
{
    public static void AddAirportDayHistoryFeature(this IServiceCollection services)
    {
        services.AddScoped<IGetAirportDayHistoryFeature, GetAirportDayHistoryFeature>();
    }
}