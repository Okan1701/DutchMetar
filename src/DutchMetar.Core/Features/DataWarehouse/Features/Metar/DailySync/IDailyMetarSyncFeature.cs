namespace DutchMetar.Core.Features.DataWarehouse.Features.Metar.DailySync;

/// <summary>
/// Feature for keeping locally stored files in sync with KNMI Data Platform.
/// This feature is mainly to fill gaps in the dataset that is stored in the database.
/// It is not the primary way to fill the database.
/// Only the last 24 hours are checked.
/// </summary>
public interface IDailyMetarSyncFeature
{
    /// <summary>
    /// Long running operation that retrieves all METAR file names stored on the KNMI Data Platform and saves it to the local database.
    /// To avoid hitting KNMI rate limits, a max request limit is enforced which will make the operation cancel.
    /// This method can be stopped and restarted without issues. It will skip already added file names.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to abort the job.</param>
    Task SyncKnmiMetarFiles(CancellationToken cancellationToken = default);
}