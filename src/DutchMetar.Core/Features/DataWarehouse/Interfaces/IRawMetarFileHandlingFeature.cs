using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;

namespace DutchMetar.Core.Features.DataWarehouse.Interfaces;

public interface IRawMetarFileHandlingFeature
{
    Task HandleFilesAsync(ICollection<KnmiFileMeta> files);
}