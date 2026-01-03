namespace DutchMetar.Core.Features.ProcessKnmiMetarFiles.Interfaces;

public interface IProcessKnmiMetarFilesFeature
{
    Task ProcessMetarFileBatchAsync(CancellationToken cancellationToken = default);
}