using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure.Contracts;

namespace DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Interfaces;

public interface IMetarFileBulkRetriever
{
    Task GetAndSaveKnmiFiles(KnmiFilesParameters parameters, CancellationToken cancellationToken,
        Guid correlationId);
}