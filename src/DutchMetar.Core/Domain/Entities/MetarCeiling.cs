using DutchMetar.Core.Domain.Enums;

namespace DutchMetar.Core.Domain.Entities;

public class MetarCeiling : Entity
{
    public int MetarId { get; set; }
    
    public Metar? Metar { get; set; }
    
    public required CeilingType Type { get; set; }
    
    public int Height { get; set; }
}