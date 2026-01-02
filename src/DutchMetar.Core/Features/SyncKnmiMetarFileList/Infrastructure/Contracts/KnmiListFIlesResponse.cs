namespace DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;

public class KnmiListFilesResponse
{
    public bool IsTruncated { get; set; }
    
    public int ResultCount { get; set; }
    
    public int MaxResults { get; set; }
    
    public required string StartAfterFilename { get; set; }
    
    public string? NextPageToken { get; set; }

    public ICollection<KnmiFileSummary> Files { get; set; } = [];
}