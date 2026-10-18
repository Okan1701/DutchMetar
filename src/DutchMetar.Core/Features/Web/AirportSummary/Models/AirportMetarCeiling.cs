using DutchMetar.Core.Domain.Enums;

namespace DutchMetar.Core.Features.Web.AirportSummary.Models;

public class AirportMetarCeiling
{
    public CeilingType Type { get; set; }
    
    public int Height { get; set; }
}