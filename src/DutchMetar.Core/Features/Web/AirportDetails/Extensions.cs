using DutchMetar.Core.Features.Web.AirportDetails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.Web.AirportDetails;

public static class Extensions
{
    public static void AddAirportDetailsFeature(this IServiceCollection services)
    {
        services.AddScoped<IGetAirportDetailsFeature, GetAirportDetailsFeature>();
    }
}