namespace DutchMetar.Core.Features.Web.AirportSummary.Models;

public class AirportForecast
{
    public required string RawTaf { get; set; }
    
    public DateTimeOffset IssuedAt { get; set; }
}