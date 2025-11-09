namespace DutchMetar.Core.Domain.Entities;

public class Airport : Entity
{
    public required string Icao { get; set; }
    
    public string? Name { get; set; }
    
    public ICollection<Metar> MetarReports { get; set; } = [];
}