using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Contracts;

namespace DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;

public interface IKnmiApiClient
{
    Task<KnmiListFilesResponse> GetDatasetFileSummaries(string dataset, KnmiFilesParameters parameters, CancellationToken cancellationToken = default);
    
    Task<string> GetDatasetFileContentAsync(string dataset, string fileName, CancellationToken cancellationToken = default);
}