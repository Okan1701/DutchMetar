using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;

namespace DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;

public interface INewKnmiFileHandler
{
    Task HandleFileAsync(KnmiFileMeta file, CancellationToken cancellationToken);
}