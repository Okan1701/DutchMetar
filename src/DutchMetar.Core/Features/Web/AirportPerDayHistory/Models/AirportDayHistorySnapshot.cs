namespace DutchMetar.Core.Features.Web.AirportPerDayHistory.Models;

public class AirportDayHistorySnapshot
{
    public required DateTimeOffset DateTime { get; set; }
    
    public int? TemperatureCelsius { get; set; }
    
    public int? DewpointCelsius { get; set; }
    
    public int? VisibilityMeters { get; set; }
    
    public int? WindSpeedKnots { get; set; }
}