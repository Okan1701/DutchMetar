namespace DutchMetar.Web.Server.Models;

public class AirportSummary
{
    public required string Icao  { get; set; }
    
    public DateTimeOffset LastIssuedMetarDate { get; set; }
    
    public bool IsAuto { get; set; }
    
    public bool IsCavok { get; set; }
    
    public int? WindDirection { get; set; }
    
    public int? WindSpeedKnots { get; set; }
}