using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;

public class KnmiRepository : IKnmiRepository
{
    private readonly IKnmiApiClient _knmiApiClient;
    private readonly ILogger<KnmiRepository> _logger;

    public KnmiRepository(IKnmiApiClient knmiApiClient, ILogger<KnmiRepository> logger)
    {
        _knmiApiClient = knmiApiClient;
        _logger = logger;
    }
    
    public async Task<ICollection<KnmiFileMeta>> GetKnmiMetarFiles(KnmiFilesParameters parameters, CancellationToken cancellationToken, Guid correlationId)
    {
        return await GetKnmiFiles(KnmiDatasetNames.Metar, parameters, cancellationToken);
    }

    public async Task<ICollection<KnmiFileMeta>> GetKnmiTafFiles(KnmiFilesParameters parameters, CancellationToken cancellationToken, Guid correlationId)
    {
        return await GetKnmiFiles(KnmiDatasetNames.Taf, parameters, cancellationToken);
    }

    private async Task<ICollection<KnmiFileMeta>> GetKnmiFiles(string dataset, KnmiFilesParameters parameters, CancellationToken cancellationToken)
    {
        // Truncated means that the last request is not the final page.
        // So we can keep retrieving the next page.
        var isTruncated = true;

        var knmiFileNames = new List<KnmiFileMeta>();
        
        // Main loop of the bulk retrieval process, retrieving all file summaries for the selected dataset.
        // This loop will continue until the API response indicates that all files have been listed.
        while (isTruncated && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogTrace("Retrieving next batch of {Dataset} files", dataset);
            
            var data = await _knmiApiClient.GetDatasetFileSummaries(dataset, parameters, cancellationToken);
            
            // If the API returns empty array, then we most likely reached the end.
            if (data.Files.Count == 0)
            {
                _logger.LogTrace("Empty file array returned from API. Aborting loop.");
                break;
            }
            
            // Append retrieved files to total list.
            var mapped = data.Files.Select(x => new KnmiFileMeta
            {
                FileName = x.Filename,
                CreatedOn = x.Created
            });
            knmiFileNames.AddRange(mapped);

            // This controls if the main loop continues.
            isTruncated = data.IsTruncated;
            
            // API result contains special token required to fetch the next page.
            parameters.NextPageToken = data.NextPageToken;
            
            // If this token is empty, then no next page is available, so we can stop.
            if (string.IsNullOrEmpty(data.NextPageToken)) break;
            
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Aborting KNMI {Dataset} sync, cancellation was requested!", dataset);
                break;
            }
        }

        return knmiFileNames;
    }
}