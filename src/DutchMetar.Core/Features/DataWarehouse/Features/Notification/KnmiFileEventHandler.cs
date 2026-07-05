using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Notification;

public class KnmiFileEventHandler : IKnmiFileEventHandler
{
    private readonly DutchMetarContext _context;
    private readonly ILogger<KnmiFileEventHandler> _logger;
    private readonly IKnmiMetarApiClient _knmiMetarApiClient;
    private readonly IMetarXmlMapper _metarXmlMapper;

    public KnmiFileEventHandler(DutchMetarContext context, ILogger<KnmiFileEventHandler> logger, IKnmiMetarApiClient knmiMetarApiClient, IMetarXmlMapper metarXmlMapper)
    {
        _context = context;
        _logger = logger;
        _knmiMetarApiClient = knmiMetarApiClient;
        _metarXmlMapper = metarXmlMapper;
    }

    public async Task HandleFileEventsAsync(ICollection<FileEvent> fileEvents)
    {

        var fileEventsToHandle = await FilterExistingFileNamesAsync(fileEvents);
        
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

    private async Task HandleSingleFileEventAsync(FileEvent fileEvent)
    {
        var fileName = fileEvent.Data?.FileName ?? string.Empty;
        if (await _context.KnmiMetarFiles.AnyAsync(x => x.FileName == fileName))
        {
            _logger.LogInformation("FileEvent received that already exists. File name = {FileName}", fileName);
            return;
        }
        
        var fileContent = await _knmiMetarApiClient.GetKnmiMetarFileContentAsync(fileName);

        if (string.IsNullOrEmpty(fileContent))
        {
            _logger.LogInformation("Downloaded file {FileName} has empty content.", fileName);
            return;
        }

        var fileCreationDate = fileEvent.Time != null ? DateTimeOffset.Parse(fileEvent.Time) : DateTimeOffset.Now;
        var fileEntity = new KnmiMetarFile
        {
            FileName = fileName,
            FileCreatedAt = fileCreationDate,
            FileContent = fileContent,
            FileLastModifiedAt = fileCreationDate,
            IsFileProcessed = false
        };

        try
        {
            var metarEntity = _metarXmlMapper.Map(fileContent);
            fileEntity.ExtractedRawMetar = metarEntity.RawMetar;
            fileEntity.IsFileProcessed = true;
            await _context.KnmiMetarFiles.AddAsync(fileEntity);
        }
        catch (MetarMappingException ex)
        {
            _logger.LogError(ex, "Failed to map XML file {FileName}", fileName);
        }
        
        _context.KnmiMetarFiles.Add(fileEntity);
        
        await _context.SaveChangesAsync();
    }

    private async Task<FileEvent[]> FilterExistingFileNamesAsync(ICollection<FileEvent> fileEvents)
    {
        var fileNamesToCheck = fileEvents
            .Where(x => x.Data?.FileName != null)
            .Where(x => x.Data?.Url != null)
            .Select(x => x.Data?.FileName)
            .Distinct();
        
        var existingFileNames = await _context.KnmiMetarFiles
            .AsNoTracking()
            .Where(f => fileNamesToCheck.Contains(f.FileName))
            .Select(f => f.FileName)
            .ToArrayAsync();
        
        return fileEvents
            .Where(x => !existingFileNames.Contains(x.Data?.FileName))
            .ToArray();
    }
}