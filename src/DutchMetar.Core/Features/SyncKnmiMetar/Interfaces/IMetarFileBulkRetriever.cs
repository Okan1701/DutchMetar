using DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure.Contracts;

namespace DutchMetar.Core.Features.SyncKnmiMetar.Interfaces;

public interface IMetarFileBulkRetriever
{
    Task GetAndSaveKnmiFiles(KnmiFilesParameters parameters, CancellationToken cancellationToken,
        Guid correlationId);
}