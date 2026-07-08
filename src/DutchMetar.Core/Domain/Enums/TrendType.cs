using System.ComponentModel;

namespace DutchMetar.Core.Domain.Enums;

public enum TrendType
{
    None = 0,
    
    [Description("No Significant Change")]
    Nosig = 1,
    
    [Description("Temporary")]
    Tempo = 2,
    
    [Description("Becoming")]
    Becmg = 3,
}