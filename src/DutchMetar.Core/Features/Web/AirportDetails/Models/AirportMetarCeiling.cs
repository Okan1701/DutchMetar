using DutchMetar.Core.Domain.Enums;

namespace DutchMetar.Core.Features.Web.AirportDetails.Models;

public class AirportMetarCeiling
{
    public CeilingType Type { get; set; }
    
    public int Height { get; set; }
}