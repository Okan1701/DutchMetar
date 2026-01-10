using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;

namespace DutchMetar.Core.Features.SyncKnmiMetarFileList.Interfaces;

public interface IMetarFileBulkRetriever
{
    Task GetAndSaveKnmiFiles(KnmiFilesParameters parameters, CancellationToken cancellationToken,
        Guid correlationId);
}