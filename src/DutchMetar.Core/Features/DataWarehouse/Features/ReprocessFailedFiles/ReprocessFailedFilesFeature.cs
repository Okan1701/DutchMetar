using System.Diagnostics;
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
        var totalProcessedFiles = 0;
        var totalFailedFiles = 0;
        var totalIgnoredFiles = 0;

        var stopwatch = new Stopwatch();

        while (knmiFiles.Any() && !cancellationToken.IsCancellationRequested)
        {
            var processedFiles = 0;
            var failedFiles = 0;
            var ignoredFiles = 0;
            stopwatch.Restart();
            foreach (var knmiFile in knmiFiles)
            {
                checkedFileIds.Add(knmiFile.Id);
                
                _context.ChangeTracker.Clear();
                _context.KnmiMetarFiles.Update(knmiFile);

                if (!string.IsNullOrEmpty(knmiFile.ExtractedRawMetar))
                {
                    try
                    {
                        DateTimeOffset date = DateTime.SpecifyKind(knmiFile.FileCreatedAt, DateTimeKind.Utc);
                        await _metarProcessor.ProcessRawMetarAsync(knmiFile.ExtractedRawMetar, null, date,
                            cancellationToken);
                        knmiFile.IsFileProcessed = true;
                        processedFiles++;
                        
                        // Save IsFileProcessed value
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    catch (MetarParseException ex)
                    {
                        _logger.LogError(ex, "Failed to re-process KNMI file {FileName}", knmiFile.FileName);
                        failedFiles++;
                    }
                }
                else ignoredFiles++;
            }
            
            totalProcessedFiles += processedFiles;
            totalIgnoredFiles += ignoredFiles;
            totalFailedFiles += failedFiles;
            _logger.LogDebug("Finished re-processing of current batch in {BatchProcessTime}s. {ProcessedFiles} / {FailedFiles} / {IgnoredFiles} files processed/failed/ignored.", stopwatch.Elapsed.Seconds, processedFiles, failedFiles, ignoredFiles);
            knmiFiles = await GetBatchAsync(checkedFileIds, cancellationToken);
        }
        
        _logger.LogDebug("Finished re-processing of failed METAR files. {ProcessedFiles} / {FailedFiles} / {IgnoredFiles} files processed/failed/ignored.", totalProcessedFiles, totalFailedFiles, totalIgnoredFiles);
        scope?.Dispose();
    }

    private async Task<KnmiMetarFile[]> GetBatchAsync(ICollection<int> excludedIds, CancellationToken cancellationToken) => await _context.KnmiMetarFiles
        .AsNoTracking()
        .OrderByDescending(file => file.CreatedAt)
        .Where(file => !file.IsFileProcessed)
        .Where(file => !excludedIds.Contains(file.Id))
        .Take(BatchSize)
        .ToArrayAsync(cancellationToken);
}