using DutchMetar.Core.Domain.Enums;

namespace DutchMetar.Core.Domain.Entities;

public class MetarImportResult : Entity
{
    public ImportResult Result { get; set; } = ImportResult.None;
    
    [Obsolete("Will be removed soon in favour of ImportResult")]
    public bool IsSuccess { get; set; }
    
    public int AddedMetarCount { get; set; }
    
    public int CorrectedMetarCount { get; set; }
    
    public int AddedAirportCount { get; set; }
    
    public string? ExceptionName { get; set; }
    
    public string? ExceptionMessage { get; set; }
    
    public string? ExceptionTrace { get; set; }
    
    public string? FailedMetarParses { get; set; }
}