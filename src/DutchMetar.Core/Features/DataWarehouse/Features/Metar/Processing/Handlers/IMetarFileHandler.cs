using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Handlers;

/// <summary>
/// Shared handler for new KNMI files that needed to be processed
/// </summary>
public interface IMetarFileHandler
{
    Task HandleFileAsync(KnmiFileMeta file, CancellationToken cancellationToken);
}