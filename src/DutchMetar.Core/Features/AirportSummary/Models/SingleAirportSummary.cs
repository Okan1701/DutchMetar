namespace DutchMetar.Core.Features.AirportSummary.Models;

public class SingleAirportSummary
{
    public required string Icao  { get; set; }
    
    public DateTime LastIssuedMetarDate { get; set; }
    
    public bool IsAuto { get; set; }
    
    public bool IsCavok { get; set; }
    
    public int? WindDirection { get; set; }
    
    public int? WindSpeedKnots { get; set; }
}