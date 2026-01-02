namespace DutchMetar.Core.Features.SyncKnmiMetarFileList.Interfaces;

public interface ISyncKnmiMetarFileListFeature
{
    /// <summary>
    /// Long running operation that retrieves all METAR file names stored on the KNMI Data Platform and saves it to the local database.
    /// To avoid hitting KNMI rate limits, a max request limit is enforced which will make the operation cancel.
    /// This method can be stopped and restarted without issues. It will skip already added file names.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to abort the job.</param>
    Task SyncKnmiFiles(CancellationToken cancellationToken = default);
}