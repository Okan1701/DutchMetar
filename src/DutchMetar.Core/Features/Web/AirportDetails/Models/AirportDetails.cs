namespace DutchMetar.Core.Features.Web.AirportDetails.Models;

public class AirportDetails
{
    public required string Icao { get; set; }

    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;
    
    public AirportCurrentMetar? LatestWeather { get; set; }
}