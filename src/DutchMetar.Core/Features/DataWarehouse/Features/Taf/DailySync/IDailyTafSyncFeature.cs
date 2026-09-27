namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.DailySync;

/// <summary>
/// Keeps locally stored TAF reports in sync with the KNMI Data Platform by checking the last 24 hours.
/// </summary>
public interface IDailyTafSyncFeature
{
    /// <summary>
    /// Retrieves and saves TAF reports published on the KNMI Data Platform.
    /// </summary>
    Task SyncKnmiTafFiles(CancellationToken cancellationToken = default);
}
