namespace DutchMetar.Core.Features.DataWarehouse.Features.ReprocessFailedFiles.Interfaces;

public interface IReprocessFailedFilesFeature
{
    Task ReprocessFailedFilesAsync(CancellationToken cancellationToken = default);
}