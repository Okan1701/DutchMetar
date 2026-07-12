namespace DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;

public class KnmiFileMeta
{
    public required string FileName { get; set; }
    
    public required DateTimeOffset CreatedOn { get; set; }
}