namespace DutchMetar.Core.Features.Web.MetarHistory;

/// <summary>
/// Request object for feature <see cref="IGetMetarHistoryFeature"/>.
/// </summary>
public class GetMetarHistoryRequest
{
    /// <summary>
    /// Airport identifier of the METAR reports.
    /// </summary>
    public required string Icao { get; set; }
    
    /// <summary>
    /// Optional
    /// </summary>
    public DateTimeOffset? StartDate { get; set; }
    
    /// <summary>
    /// Optional
    /// </summary>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// Optional size of a page
    /// </summary>
    public int? PageSize { get; set; } = GetMetarHistoryFeature.DefaultPageSize;

    /// <summary>
    /// Current page
    /// </summary>
    public int Page { get; set; } = 1;
}