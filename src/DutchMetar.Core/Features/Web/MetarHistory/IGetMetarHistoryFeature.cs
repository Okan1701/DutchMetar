namespace DutchMetar.Core.Features.Web.MetarHistory;

/// <summary>
/// Feature for retrieving historical list of METAR reports for a specific ICAO station.
/// </summary>
public interface IGetMetarHistoryFeature
{
    Task<GetMetarHistoryResult> GetHistoryAsync(GetMetarHistoryRequest request, CancellationToken cancellationToken = default);
}