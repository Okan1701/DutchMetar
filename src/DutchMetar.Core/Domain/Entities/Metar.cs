using System.ComponentModel.DataAnnotations;
using DutchMetar.Core.Domain.Constants;

namespace DutchMetar.Core.Domain.Entities;

public class Metar : Entity
{
    public int AirportId { get; set; }

    public Airport? Airport { get; set; }
    
    public bool IsAuto { get; set; }
    
    public bool IsCavok { get; set; }
    
    public bool IsCorrected { get; set; }
    
    public int? WindDirection { get; set; }
    
    public int? WindSpeedKnots { get; set; }
    
    public int? WindSpeedGustsKnots { get; set; }
    
    public bool NoCloudsDetected { get; set; }
    
    public int? VisibilityMeters { get; set; }
    
    [MaxLength(EntityConstants.DefaultMaxStringLength)]
    public required string RawMetar { get; set; }
    
    public int? TemperatureCelsius { get; set; }
    
    public int? DewpointCelsius { get; set; }
    
    public int? AltimeterValue { get; set; }
    
    [MaxLength(EntityConstants.DefaultMaxStringLength)]
    public string? Remarks { get; set; }
    
    public DateTime IssuedAt { get; set; }
    
    public ICollection<MetarCeiling>? Ceilings { get; set; }
}