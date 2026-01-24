using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Features.ReprocessFailedFiles.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using DutchMetar.Core.Infrastructure.Accessors;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.ReprocessFailedFiles;

public class ReprocessFailedFilesFeature : IReprocessFailedFilesFeature
{
    private readonly DutchMetarContext _context;
    private readonly ILogger<ReprocessFailedFilesFeature> _logger;
    private readonly IMetarProcessor _metarProcessor;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    private const int BatchSize = 1000;

    public ReprocessFailedFilesFeature(DutchMetarContext context, ILogger<ReprocessFailedFilesFeature> logger, IMetarProcessor metarProcessor, ICorrelationIdAccessor correlationIdAccessor)
    {
        _context = context;
        _logger = logger;
        _metarProcessor = metarProcessor;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public async Task ReprocessFailedFilesAsync(CancellationToken cancellationToken = default)
    {
        var scope = _logger.BeginScope(new KeyValuePair<string, object?>[]
        {
            new("CorrelationId", _correlationIdAccessor.CorrelationId),
            new("ReprocessFDateTimeUtc", DateTime.UtcNow),
        });
        _logger.LogInformation("Starting re-processing of failed files.");
        
        // Keep a track of files that were checked
        // Otherwise we get an infinite loop if 1 or more files cannot be re-processed
        var checkedFileIds = new List<int>();
        
        // Process files in batches
        var knmiFiles = await GetBatchAsync(checkedFileIds, cancellationToken);
        var processedFiles = 0;

        while (knmiFiles.Any() && !cancellationToken.IsCancellationRequested)
        {
            foreach (var knmiFile in knmiFiles)
            {
                checkedFileIds.Add(knmiFile.Id);
                
                if (!string.IsNullOrEmpty(knmiFile.ExtractedRawMetar))
                {
                    try
                    {
                        DateTimeOffset date = DateTime.SpecifyKind(knmiFile.FileCreatedAt, DateTimeKind.Utc);
                        await _metarProcessor.ProcessRawMetarAsync(knmiFile.ExtractedRawMetar, null, date,
                            cancellationToken);
                        knmiFile.IsFileProcessed = true;
                        processedFiles++;
                    }
                    catch (MetarParseException ex)
                    {
                        _logger.LogError(ex, "Failed to re-process KNMI file {FileName}", knmiFile.FileName);
                        knmiFile.IsFileProcessed = false;
                    }
                }
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            knmiFiles = await GetBatchAsync(checkedFileIds, cancellationToken);
        }
        
        _logger.LogInformation("Finished re-processing of failed files. {ProcessedCount} files have been processed.", processedFiles);
        scope?.Dispose();
    }

    private async Task<KnmiMetarFile[]> GetBatchAsync(ICollection<int> excludedIds, CancellationToken cancellationToken) => await _context.KnmiMetarFiles
        .OrderByDescending(file => file.CreatedAt)
        .Where(file => !file.IsFileProcessed)
        .Where(file => !excludedIds.Contains(file.Id))
        .Take(BatchSize)
        .ToArrayAsync(cancellationToken);
}