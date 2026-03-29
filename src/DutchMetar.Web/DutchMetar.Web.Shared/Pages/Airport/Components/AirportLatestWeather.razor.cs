using DutchMetar.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Shared.Pages.Airport.Components;

public partial class AirportLatestWeather : ComponentBase
{
    [Parameter]
    public required AirportDetails AirportDetails { get; set; }
    
    private string FormatWindDirection()
    {
        if (AirportDetails?.LatestWeather is { WindSpeedKnots: not null, WindDirection: not null })
        {
            return $"{AirportDetails.LatestWeather.WindDirection} / {AirportDetails.LatestWeather.WindSpeedKnots} kt";
        }

        return "N/A";
    }
}