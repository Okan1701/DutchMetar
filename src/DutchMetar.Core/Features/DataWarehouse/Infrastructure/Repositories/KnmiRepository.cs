using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.Contracts.DataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;

public class KnmiRepository : IKnmiRepository
{
    private readonly IKnmiMetarApiClient _knmiMetarApiClient;
    private readonly DutchMetarContext _dutchMetarContext;
    private readonly ILogger<KnmiRepository> _logger;

    public KnmiRepository(IKnmiMetarApiClient knmiMetarApiClient, DutchMetarContext dutchMetarContext, ILogger<KnmiRepository> logger)
    {
        _knmiMetarApiClient = knmiMetarApiClient;
        _dutchMetarContext = dutchMetarContext;
        _logger = logger;
    }
    
    public async Task<ICollection<KnmiFileMeta>> GetKnmiMetarFiles(KnmiFilesParameters parameters, CancellationToken cancellationToken, Guid correlationId)
    {
        // Truncated means that the last request is not the final page.
        // So we can keep retrieving the next page.
        var isTruncated = true;

        var knmiFileNames = new List<KnmiFileMeta>();
        
        // Main loop of the bulk retrieval process.
        // This will in essence retrieve a list of files, retrieve content of each file and process it into Airport and Metar entities.
        // This loop will continue until the API response indicates that all files have been listed.
        while (isTruncated && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogTrace("Retrieving next batch of metar files");
            
            // Get list of available METAR files.
            var data = await _knmiMetarApiClient.GetMetarFileSummaries(parameters, cancellationToken);
            
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
                _logger.LogWarning("Aborting KNMI metar sync, cancellation was requested!");
                break;
            }
        }

        return knmiFileNames;
    }
}