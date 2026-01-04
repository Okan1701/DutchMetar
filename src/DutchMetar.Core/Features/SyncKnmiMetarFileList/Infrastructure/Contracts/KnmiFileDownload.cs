namespace DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;

/// <summary>
/// Response when retrieving file url
/// See also https://tyk-cdn.dataplatform.knmi.nl/open-data/index.html
/// </summary>
public class KnmiFileDownload
{
    public required string ContentType { get; set; }
    
    public DateTimeOffset LastModified { get; set; }
    
    public required string Size { get; set; }
    
    public required string TemporaryDownloadUrl { get; set; }
}