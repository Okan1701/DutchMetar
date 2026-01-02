namespace DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;

/// <summary>
/// Represents supported parameters by the KNMI Data API.
/// See also: https://tyk-cdn.dataplatform.knmi.nl/open-data/index.html?
/// </summary>
public class KnmiFilesParameters
{
    /// <summary>
    /// Used with <see cref="End"/> to specify a date range.
    /// </summary>
    public DateTime? Begin { get; set; }
    
    /// <summary>
    /// Used with <see cref="Start"/> to specify a date range.
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// Max supported value: 1000
    /// </summary>
    public int? MaxKeys { get; set; } = 1000;
    
    /// <summary>
    /// Token to retrieve the next page. If a response is truncated, it will contain a nextToken field.
    /// </summary>
    public string? NextPageToken { get; set; }
    
    /// <summary>
    /// Supported string values: asc or desc
    /// </summary>
    public string? Sorting { get; set; }
    
    /// <summary>
    /// Supported string values: created
    /// </summary>
    public string? OrderBy { get; set; }
}