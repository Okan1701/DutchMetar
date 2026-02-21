namespace DutchMetar.Core.Features.AirportPerDayHistory.Models;

public class GetAirportDayHistoryInput
{
    public required string Icao { get; set; }
    
    public required DateOnly Date { get; set; }
}