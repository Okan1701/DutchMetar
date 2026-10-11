namespace DutchMetar.Core.Features.Web.TafHistory;

public class GetTafHistoryResultReport
{
    public required int TafId { get; set; }

    public required string RawTaf { get; set; }

    public DateTimeOffset? IssuedAt { get; set; }
}
