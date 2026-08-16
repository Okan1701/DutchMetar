using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications.Contracts;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.Notifications;

public class NewTafNotificationFeature : INewTafNotificationFeature
{
    private readonly IKnmiApiClient _knmiApiClient;
    private readonly ILogger<NewTafNotificationFeature> _logger;
    private readonly DutchMetarContext _context;
    private readonly IRawTafFileParser _tafFileParser;

    public NewTafNotificationFeature(IKnmiApiClient knmiApiClient, ILogger<NewTafNotificationFeature> logger, DutchMetarContext context, IRawTafFileParser tafFileParser)
    {
        _knmiApiClient = knmiApiClient;
        _logger = logger;
        _context = context;
        _tafFileParser = tafFileParser;
    }

    public bool CanHandleMessage(FileEvent fileEvent)
    {
        return fileEvent.Data?.DataSetName == KnmiDatasetNames.Taf;
    }

    public async Task HandleNotificationAsync(FileEvent fileEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling new TAF notification: {FileName}", fileEvent.Data?.FileName);
        if (string.IsNullOrEmpty(fileEvent.Data?.FileName))
        {
            _logger.LogWarning("Received new TAF FileEvent with an empty FileName!");
            return;
        }
        
        var fileContent = await _knmiApiClient.GetDatasetFileContentAsync(KnmiDatasetNames.Taf, fileEvent.Data.FileName, cancellationToken);

        if (string.IsNullOrEmpty(fileContent))
        {
            _logger.LogWarning("Downloaded TAF file has empty content!");
            return;
        }

        Domain.Entities.Taf tafEntity;
        try
        {
            tafEntity = _tafFileParser.ParseRawTafToEntity(fileContent);
        }
        catch (TafParsingException ex)
        {
            _logger.LogError(ex, "Failed to parse raw TAF message: {FileName}", fileEvent.Data.DataSetName);
            return;
        }

        var icaoNormalized = tafEntity.Airport?.Icao.ToUpperInvariant() ?? string.Empty;
        var existingAirportEntity = await _context.Airports.FirstOrDefaultAsync(x => x.Icao == icaoNormalized, cancellationToken);

        if (existingAirportEntity != null)
        {
            _logger.LogDebug("TAF Airport already exists in database.");
            tafEntity.Airport = existingAirportEntity;
        }
        
        _context.Tafs.Add(tafEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}