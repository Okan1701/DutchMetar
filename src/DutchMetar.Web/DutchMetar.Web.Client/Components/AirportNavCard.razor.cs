using DutchMetar.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Components;

public partial class AirportNavCard : ComponentBase
{
    [Parameter]
    public required AirportSummary AirportSummary { get; set; }

    private string FormatWindData(int? windDirection, int? windSpeed)
    {
        if (windDirection.HasValue && windSpeed.HasValue)
        {
            return $"{windDirection}° / {windSpeed} kt";
        }

        if (windDirection.HasValue && !windSpeed.HasValue)
        {
            return $"{windDirection}° (no wind speed avail.)";
        }
        
        if (!windDirection.HasValue && windSpeed.HasValue)
        {
            return $"{windSpeed} kt (no wind direction avail.)";
        }
        
        return "No wind data avail.";
    }
}