namespace DutchMetar.Core.Features.Web.MetarHistory;

public class GetMetarHistoryResultReports
{
    public required  int MetarId { get; set; }
    
    public required string RawMetar { get; set; }
}