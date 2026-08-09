using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Parsers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Handlers;

public class MetarFileHandler : IMetarFileHandler
{
    private readonly DutchMetarContext _context;
    private readonly ILogger<MetarFileHandler> _logger;
    private readonly IKnmiApiClient _knmiApiClient;
    private readonly IMetarXmlParser _metarXmlParser;

    public MetarFileHandler(DutchMetarContext context, ILogger<MetarFileHandler> logger, IKnmiApiClient knmiApiClient, IMetarXmlParser metarXmlParser)
    {
        _context = context;
        _logger = logger;
        _knmiApiClient = knmiApiClient;
        _metarXmlParser = metarXmlParser;
    }

    public async Task HandleFileAsync(KnmiFileMeta fileMeta, CancellationToken cancellationToken)
    {
        if (await _context.KnmiMetarFiles.AnyAsync(x => x.FileName == fileMeta.FileName, cancellationToken))
        {
            _logger.LogInformation("FileEvent received that already exists. File name = {FileName}", fileMeta.FileName);
            return;
        }
        
        var fileContent = await _knmiApiClient.GetDatasetFileContentAsync(KnmiDatasetNames.Metar, fileMeta.FileName, cancellationToken);

        if (string.IsNullOrEmpty(fileContent))
        {
            _logger.LogInformation("Downloaded file {FileName} has empty content.", fileMeta.FileName);
            return;
        }
        
        var fileEntity = new KnmiMetarFile
        {
            FileName = fileMeta.FileName,
            FileCreatedAt = fileMeta.CreatedOn,
            FileContent = fileContent,
            FileLastModifiedAt = fileMeta.CreatedOn,
            IsFileProcessed = false
        };
        _context.KnmiMetarFiles.Add(fileEntity);

        try
        {
            var metarEntity = _metarXmlParser.Map(fileContent);

            if (metarEntity?.Airport?.Icao != null)
            {
                var existingAirport = await _context.Airports.FirstOrDefaultAsync(x => x.Icao == metarEntity.Airport.Icao, cancellationToken);

                if (existingAirport != null)
                {
                    metarEntity.Airport = existingAirport;
                }
            
                fileEntity.ExtractedRawMetar = metarEntity.RawMetar;
                fileEntity.IsFileProcessed = true;
                _context.Metars.Add(metarEntity);
            }
        }
        catch (MetarXmlParsingException ex)
        {
            _logger.LogError(ex, "Failed to map XML file {FileName}", fileMeta.FileName);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}