namespace DutchMetar.Web.Shared.Models;

public class AirportDetails
{
    public required string Icao { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    
    public AirportCurrentMetar? CurrentWeather { get; set; }
}