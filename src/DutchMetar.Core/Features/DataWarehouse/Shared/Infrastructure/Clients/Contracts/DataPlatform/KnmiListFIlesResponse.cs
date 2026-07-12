namespace DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.Contracts.DataPlatform;

public class KnmiListFilesResponse
{
    public bool IsTruncated { get; set; }
    
    public int ResultCount { get; set; }
    
    public int MaxResults { get; set; }
    
    public required string StartAfterFilename { get; set; }
    
    public string? NextPageToken { get; set; }

    public ICollection<KnmiFileSummary> Files { get; set; } = [];
}