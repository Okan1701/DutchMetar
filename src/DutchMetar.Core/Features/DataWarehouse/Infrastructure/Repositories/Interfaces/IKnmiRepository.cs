using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Contracts;

namespace DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;

public interface IKnmiRepository
{
    Task<ICollection<KnmiFileMeta>> GetKnmiMetarFiles(KnmiFilesParameters parameters, CancellationToken cancellationToken,
        Guid correlationId);
}