using System.ComponentModel.DataAnnotations;
using DutchMetar.Core.Domain.Constants;

namespace DutchMetar.Core.Domain.Entities;

public class Taf : Entity
{
    public int AirportId { get; set; }

    public Airport? Airport { get; set; }
    
    public DateTimeOffset? IssuedAt { get; set; }
    
    [MaxLength(EntityConstants.DefaultMaxStringLength)]
    public required string RawTaf { get; set; }
}