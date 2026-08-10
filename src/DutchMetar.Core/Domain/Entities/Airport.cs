using System.ComponentModel.DataAnnotations;
using DutchMetar.Core.Domain.Constants;

namespace DutchMetar.Core.Domain.Entities;

public class Airport : Entity
{
    [MaxLength(EntityConstants.DefaultMaxStringLength)]
    public required string Icao { get; set; }
    
    [MaxLength(EntityConstants.DefaultMaxStringLength)]
    public string? Name { get; set; }
    
    public ICollection<Metar> MetarReports { get; set; } = [];
    
    public ICollection<Taf> TafReports { get; set; } = [];
}