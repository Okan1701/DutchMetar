namespace DutchMetar.Core.Domain.Entities;

public class MetarImportResult : Entity
{
    public bool IsSuccess { get; set; }
    
    public int AddedMetarCount { get; set; }
    
    public int CorrectedMetarCount { get; set; }
    
    public int AddedAirportCount { get; set; }
    
    public string? ExceptionName { get; set; }
    
    public string? ExceptionMessage { get; set; }
    
    public string? ExceptionTrace { get; set; }
}