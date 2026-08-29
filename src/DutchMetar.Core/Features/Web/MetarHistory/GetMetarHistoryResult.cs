namespace DutchMetar.Core.Features.Web.MetarHistory;

public class GetMetarHistoryResult
{
    public required string Icao { get; set; }
    
    public string? AirportName { get; set; }
    
    public int CurrentPage { get; set; }
    
    public int MaxPages { get; set; }
    
    public int TotalItems { get; set; }

    public ICollection<GetMetarHistoryResultReports> MetarReports { get; set; } = [];
}