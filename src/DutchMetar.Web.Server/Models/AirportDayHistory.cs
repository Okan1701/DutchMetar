namespace DutchMetar.Web.Server.Models;

public class AirportDayHistory
{
    public required string Icao { get; set; }
    
    public bool IsMissingData { get; set; }
    
    public ICollection<AirportDayHistorySnapshot> History { get; set; } = Array.Empty<AirportDayHistorySnapshot>();
}