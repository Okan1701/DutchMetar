namespace DutchMetar.Web.Shared.Models;

public class AirportSummary
{
    public required string Icao  { get; set; }
    
    public DateTime LastIssuedMetarDate { get; set; }
    
    public bool IsAuto { get; set; }
    
    public bool IsCavok { get; set; }
    
    public int? WindDirection { get; set; }
    
    public int? WindSpeedKnots { get; set; }
}