using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Shared;

public class NewKnmiFileHandler : INewKnmiFileHandler
{
    private readonly DutchMetarContext _context;
    private readonly ILogger<NewKnmiFileHandler> _logger;
    private readonly IKnmiMetarApiClient _knmiMetarApiClient;
    private readonly IMetarXmlMapper _metarXmlMapper;

    public NewKnmiFileHandler(DutchMetarContext context, ILogger<NewKnmiFileHandler> logger, IKnmiMetarApiClient knmiMetarApiClient, IMetarXmlMapper metarXmlMapper)
    {
        _context = context;
        _logger = logger;
        _knmiMetarApiClient = knmiMetarApiClient;
        _metarXmlMapper = metarXmlMapper;
    }

    public async Task HandleFileAsync(KnmiFileMeta fileMeta, CancellationToken cancellationToken)
    {
        if (await _context.KnmiMetarFiles.AnyAsync(x => x.FileName == fileMeta.FileName, cancellationToken))
        {
            _logger.LogInformation("FileEvent received that already exists. File name = {FileName}", fileMeta.FileName);
            return;
        }
        
        var fileContent = await _knmiMetarApiClient.GetKnmiMetarFileContentAsync(fileMeta.FileName, cancellationToken);

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
            var metarEntity = _metarXmlMapper.Map(fileContent);

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
        catch (MetarMappingException ex)
        {
            _logger.LogError(ex, "Failed to map XML file {FileName}", fileMeta.FileName);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}