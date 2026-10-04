namespace DutchMetar.Core.Features.Web.TafHistory;

public class GetTafHistoryRequest
{
    public required string Icao { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public int? PageSize { get; set; } = GetTafHistoryFeature.DefaultPageSize;

    public int Page { get; set; } = 1;
}
