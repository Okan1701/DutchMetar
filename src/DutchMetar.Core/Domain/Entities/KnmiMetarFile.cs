using System.ComponentModel.DataAnnotations;
using DutchMetar.Core.Domain.Constants;

namespace DutchMetar.Core.Domain.Entities;

public class KnmiMetarFile : Entity
{
    [MaxLength(EntityConstants.DefaultMaxStringLength)]
    public required string FileName { get; set; }
    
    public required DateTime FileCreatedAt { get; set; }
    
    public required DateTime FileLastModifiedAt { get; set; }

    public bool IsFileProcessed { get; set; }
}