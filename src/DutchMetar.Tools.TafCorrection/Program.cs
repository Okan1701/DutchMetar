using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

const string connectionString = "Server=mssql.hc.emeni.nl;Database=dutchmetar_staging;Trusted_Connection=True;";
const int batchSize = 500;

using var cancellationTokenSource = new CancellationTokenSource();
ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};
Console.CancelKeyPress += cancelHandler;

try
{
    var options = new DbContextOptionsBuilder<DutchMetarContext>()
        .UseSqlServer(connectionString)
        .Options;
    await using var context = new DutchMetarContext(options);
    var parser = new RawTafFileParser();
    var maximumId = await context.Tafs.MaxAsync(taf => (int?)taf.Id, cancellationTokenSource.Token) ?? 0;
    var lastId = 0;
    var correctedCount = 0;
    var unchangedCount = 0;
    var skippedCount = 0;

    while (true)
    {
        cancellationTokenSource.Token.ThrowIfCancellationRequested();
        var batch = await context.Tafs
            .Where(taf => taf.Id > lastId && taf.Id <= maximumId)
            .GroupJoin(
                context.KnmiTafFiles,
                taf => taf.SourceFileName,
                file => file.FileName,
                (taf, files) => new { Taf = taf, Files = files })
            .SelectMany(
                row => row.Files.DefaultIfEmpty(),
                (row, file) => new
                {
                    Taf = row.Taf,
                    FileCreatedAt = file == null ? (DateTimeOffset?)null : file.FileCreatedAt,
                    FileContent = file == null ? null : file.FileContent,
                    ExtractedRawTaf = file == null ? null : file.ExtractedRawTaf
                })
            .OrderBy(row => row.Taf.Id)
            .Take(batchSize)
            .ToListAsync(cancellationTokenSource.Token);

        if (batch.Count == 0)
        {
            break;
        }

        foreach (var row in batch)
        {
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            if (row.FileCreatedAt is null)
            {
                skippedCount++;
                Console.Error.WriteLine(
                    $"Skipping TAF {row.Taf.Id}: no KNMI TAF file matches source file '{row.Taf.SourceFileName}'.");
                continue;
            }

            if (row.FileCreatedAt.Value == default)
            {
                skippedCount++;
                Console.Error.WriteLine($"Skipping TAF {row.Taf.Id}: the KNMI file creation date is missing.");
                continue;
            }

            var fileContent = string.IsNullOrWhiteSpace(row.FileContent) ? row.ExtractedRawTaf : row.FileContent;
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                skippedCount++;
                Console.Error.WriteLine($"Skipping TAF {row.Taf.Id}: the matching KNMI TAF file has no content.");
                continue;
            }

            Taf parsedTaf;
            try
            {
                parsedTaf = parser.ParseRawTafToEntity(fileContent, row.FileCreatedAt.Value);
                var storedTaf = parser.ParseRawTafToEntity(row.Taf.RawTaf, row.FileCreatedAt.Value);
                if (!parsedTaf.RawTaf.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                        .SequenceEqual(storedTaf.RawTaf.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries),
                            StringComparer.OrdinalIgnoreCase))
                {
                    skippedCount++;
                    Console.Error.WriteLine($"Skipping TAF {row.Taf.Id}: its payload differs from the matching KNMI file.");
                    continue;
                }
            }
            catch (TafParsingException exception)
            {
                skippedCount++;
                Console.Error.WriteLine($"Skipping TAF {row.Taf.Id}: {exception.Message}");
                continue;
            }

            if (parsedTaf.IssuedAt is null)
            {
                skippedCount++;
                Console.Error.WriteLine($"Skipping TAF {row.Taf.Id}: its issue date/time could not be parsed.");
                continue;
            }

            if (row.Taf.IssuedAt == parsedTaf.IssuedAt)
            {
                unchangedCount++;
                continue;
            }

            row.Taf.IssuedAt = parsedTaf.IssuedAt;
            correctedCount++;
        }

        await context.SaveChangesAsync(cancellationTokenSource.Token);
        lastId = batch[^1].Taf.Id;
        context.ChangeTracker.Clear();
        Console.WriteLine($"Saved TAF records through ID {lastId}.");
    }

    Console.WriteLine(
        $"TAF correction finished. Corrected: {correctedCount}; unchanged: {unchangedCount}; skipped: {skippedCount}.");
    return skippedCount == 0 ? 0 : 2;
}
catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
{
    Console.Error.WriteLine("TAF correction was cancelled. Previously saved batches remain committed; rerunning is safe.");
    return 130;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"TAF correction failed: {exception}");
    return 1;
}
finally
{
    Console.CancelKeyPress -= cancelHandler;
}
