namespace DutchMetar.Core.Features.ProcessKnmiMetarFiles.Infrastructure.Interfaces;

/// <summary>
/// Client for downloading METAR XML files from KNMI Data platform
/// This client is for operating on the 'metar' dataset.
/// See also: https://dataplatform.knmi.nl/dataset/metar-1-0
/// </summary>
public interface IKnmiMetarFileClient
{
    /// <summary>
    /// Gets the string content of the specified <param name="fileName"></param>
    /// </summary>
    /// <param name="fileName">Filename to retrieve the contents of.</param>
    /// <param name="cancellationToken">(Optional) cancellation token to abort the operation.</param>
    /// <returns></returns>
    Task<string> GetKnmiMetarFileContentAsync(string fileName, CancellationToken cancellationToken = default);
}