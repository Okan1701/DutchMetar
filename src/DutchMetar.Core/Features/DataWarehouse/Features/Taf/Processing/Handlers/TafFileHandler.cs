using DutchMetar.Core.Domain.Constants;
using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.Processing.Handlers;

public class TafFileHandler : ITafFileHandler
{
    private readonly DutchMetarContext _context;
    private readonly ILogger<TafFileHandler> _logger;
    private readonly IKnmiApiClient _knmiApiClient;
    private readonly IRawTafFileParser _tafFileParser;

    public TafFileHandler(
        DutchMetarContext context,
        ILogger<TafFileHandler> logger,
        IKnmiApiClient knmiApiClient,
        IRawTafFileParser tafFileParser)
    {
        _context = context;
        _logger = logger;
        _knmiApiClient = knmiApiClient;
        _tafFileParser = tafFileParser;
    }

    public async Task HandleFileAsync(KnmiFileMeta fileMeta, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileMeta.FileName))
        {
            _logger.LogWarning("Received TAF file metadata with an empty file name.");
            return;
        }

        if (await _context.KnmiTafFiles.AnyAsync(
                x => x.FileName == fileMeta.FileName, cancellationToken))
        {
            _logger.LogInformation("TAF file already exists. File name = {FileName}", fileMeta.FileName);
            return;
        }

        var fileContent = await _knmiApiClient.GetDatasetFileContentAsync(
            KnmiDatasetNames.Taf, fileMeta.FileName, cancellationToken);

        if (string.IsNullOrEmpty(fileContent))
        {
            _logger.LogInformation("Downloaded TAF file {FileName} has empty content.", fileMeta.FileName);
            return;
        }

        var fileEntity = new KnmiTafFile
        {
            FileName = fileMeta.FileName,
            FileCreatedAt = fileMeta.CreatedOn,
            FileLastModifiedAt = fileMeta.CreatedOn,
            FileContent = fileContent,
            IsFileProcessed = false
        };
        _context.KnmiTafFiles.Add(fileEntity);
        await _context.SaveChangesAsync(cancellationToken);

        Domain.Entities.Taf tafEntity;
        try
        {
            tafEntity = _tafFileParser.ParseRawTafToEntity(fileContent);
        }
        catch (TafParsingException ex)
        {
            _logger.LogError(ex, "Failed to parse raw TAF message: {FileName}", fileMeta.FileName);
            return;
        }

        tafEntity.Source = DataSourceConstants.Knmi;
        tafEntity.SourceFileName = fileMeta.FileName;

        var icaoNormalized = tafEntity.Airport?.Icao.ToUpperInvariant() ?? string.Empty;
        var existingAirportEntity = await _context.Airports.FirstOrDefaultAsync(
            x => x.Icao == icaoNormalized, cancellationToken);

        if (existingAirportEntity != null)
        {
            tafEntity.Airport = existingAirportEntity;
        }

        fileEntity.ExtractedRawTaf = tafEntity.RawTaf;
        fileEntity.IsFileProcessed = true;
        _context.Tafs.Add(tafEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
