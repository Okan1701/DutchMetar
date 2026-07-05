using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DutchMetar.Core.Domain.Constants;

namespace DutchMetar.Core.Domain.Entities;

public class KnmiMetarFile : Entity
{
    [MaxLength(EntityConstants.DefaultMaxStringLength)]
    public required string FileName { get; set; }
    
    public required DateTimeOffset FileCreatedAt { get; set; }
    
    public required DateTimeOffset FileLastModifiedAt { get; set; }

    public bool IsFileProcessed { get; set; }
    
    [MaxLength(EntityConstants.DefaultMaxStringLength)]
    public string? ExtractedRawMetar { get; set; }
    
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? FileContent { get; set; }
}