using DutchMetar.Core.Features.AIrportDetails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Features.AirportDetails;

public static class Extensions
{
    public static void AddAirportDetailsFeature(this IServiceCollection services)
    {
        services.AddScoped<IGetAirportDetailsFeature, GetAirportDetailsFeature>();
    }
}