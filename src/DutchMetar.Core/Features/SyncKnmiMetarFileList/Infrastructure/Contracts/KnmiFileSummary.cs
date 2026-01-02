using DutchMetar.Core.Domain.Entities;

namespace DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;

public class KnmiFileSummary
{
    public required string Filename { get; set; }
    
    public DateTime LastModified { get; set; }
    
    public int Size { get; set; }
    
    public DateTime Created { get; set; }

    public KnmiMetarFile ToKnmiMetarFileEntity() => new KnmiMetarFile
    {
        FileName = Filename,
        FileCreatedAt = Created,
        FileLastModifiedAt = LastModified
    };
}