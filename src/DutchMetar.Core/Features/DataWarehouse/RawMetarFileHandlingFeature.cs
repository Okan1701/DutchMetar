using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse;

public class RawMetarFileHandlingFeature : IRawMetarFileHandlingFeature
{
    private readonly DutchMetarContext _context;
    private readonly ILogger<RawMetarFileHandlingFeature> _logger;
    private readonly IKnmiMetarApiClient _knmiMetarApiClient;
    private readonly IMetarXmlMapper _metarXmlMapper;

    public RawMetarFileHandlingFeature(DutchMetarContext context, ILogger<RawMetarFileHandlingFeature> logger, IKnmiMetarApiClient knmiMetarApiClient, IMetarXmlMapper metarXmlMapper)
    {
        _context = context;
        _logger = logger;
        _knmiMetarApiClient = knmiMetarApiClient;
        _metarXmlMapper = metarXmlMapper;
    }

    public async Task HandleFilesAsync(ICollection<KnmiFileMeta> files)
    {

        var fileEventsToHandle = await FilterExistingFileNamesAsync(files);
        
        if (fileEventsToHandle.Length == 0)
        {
            _logger.LogTrace("No new FileEvents to handle.");
            return;
        }
        
        foreach (var fileEvent in fileEventsToHandle)
        {
            try
            {
                await HandleSingleFileEventAsync(fileEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle FileEvent.");
            }
        }
    }

    private async Task HandleSingleFileEventAsync(KnmiFileMeta fileMeta)
    {
        if (await _context.KnmiMetarFiles.AnyAsync(x => x.FileName == fileMeta.FileName))
        {
            _logger.LogInformation("FileEvent received that already exists. File name = {FileName}", fileMeta.FileName);
            return;
        }
        
        var fileContent = await _knmiMetarApiClient.GetKnmiMetarFileContentAsync(fileMeta.FileName);

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
                var existingAirport = await _context.Airports.FirstOrDefaultAsync(x => x.Icao == metarEntity.Airport.Icao);

                if (existingAirport != null)
                {
                    metarEntity.Airport = existingAirport;
                }
            
                fileEntity.ExtractedRawMetar = metarEntity.RawMetar;
                fileEntity.IsFileProcessed = true;
                _context.Metars.Add(metarEntity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map XML file {FileName}", fileMeta.FileName);
        }
        
        _context.KnmiMetarFiles.Add(fileEntity);
        
        await _context.SaveChangesAsync();
    }

    private async Task<KnmiFileMeta[]> FilterExistingFileNamesAsync(ICollection<KnmiFileMeta> files)
    {
        var fileNamesToCheck = files
            .Select(x => x.FileName)
            .Distinct();
        
        var existingFileNames = await _context.KnmiMetarFiles
            .AsNoTracking()
            .Where(f => fileNamesToCheck.Contains(f.FileName))
            .Select(f => f.FileName)
            .ToArrayAsync();
        
        return files
            .Where(x => !existingFileNames.Contains(x.FileName))
            .ToArray();
    }
}