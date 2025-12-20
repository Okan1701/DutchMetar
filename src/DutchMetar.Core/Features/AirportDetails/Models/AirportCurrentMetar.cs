namespace DutchMetar.Core.Features.AirportDetails.Models;

public class AirportCurrentMetar
{
    public required string RawMetar { get; set; } = "NIL";
    
    public bool IsAuto { get; set; }
    
    public bool IsCavok { get; set; }
    
    public bool IsCorrected { get; set; }
    
    public int? WindDirection { get; set; }
    
    public int? WindSpeedKnots { get; set; }
    
    public int? WindSpeedGustsKnots { get; set; }
    
    public bool NoCloudsDetected { get; set; }
    
    public int? VisibilityMeters { get; set; }
    
    public int? TemperatureCelsius { get; set; }
    
    public int? DewpointCelsius { get; set; }
    
    public int? AltimeterValue { get; set; }
    
    public string? Remarks { get; set; }
}