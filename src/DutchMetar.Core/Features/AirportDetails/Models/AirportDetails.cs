namespace DutchMetar.Core.Features.AirportDetails.Models;

public class AirportDetails
{
    public required string Icao { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    
    public AirportCurrentMetar? CurrentWeather { get; set; }
    
    public ICollection<AirportCurrentMetar> HistoricalWeather { get; set; } = Array.Empty<AirportCurrentMetar>();
}