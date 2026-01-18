using DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure.Contracts;

namespace DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure.Interfaces;

/// <summary>
/// Client for interacting with the KNMI Data Platform.
/// This client is for operating on the 'metar' dataset.
/// See also: https://dataplatform.knmi.nl/dataset/metar-1-0
/// </summary>
public interface IKnmiMetarApiClient
{
    /// <summary>
    /// Retrieve a list of METAR files that are available.
    /// These are only the file names. Retrieving the contents is a seperate API endpoint.
    /// </summary>
    /// <param name="parameters">Supported API parameters</param>
    /// <param name="cancellationToken">(Optional) cancellation token to abort the operation.</param>
    /// <returns><see cref="KnmiListFilesResponse"/></returns>
    Task<KnmiListFilesResponse> GetMetarFileSummaries(KnmiFilesParameters parameters, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the string content of the specified <param name="fileName"></param>
    /// </summary>
    /// <param name="fileName">Filename to retrieve the contents of.</param>
    /// <param name="cancellationToken">(Optional) cancellation token to abort the operation.</param>
    /// <returns></returns>
    Task<string> GetKnmiMetarFileContentAsync(string fileName, CancellationToken cancellationToken = default);
}