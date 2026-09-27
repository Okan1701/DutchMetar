using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.Processing.Handlers;

public interface ITafFileHandler
{
    Task HandleFileAsync(KnmiFileMeta fileMeta, CancellationToken cancellationToken);
}
