using System.Text.RegularExpressions;
using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.LoadDutchMetars.Interfaces;
using DutchMetar.Core.Features.ProcessKnmiMetarFiles.Infrastructure.Interfaces;
using DutchMetar.Core.Features.ProcessKnmiMetarFiles.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using MetarParserCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Metar = MetarParserCore.Objects.Metar;

namespace DutchMetar.Core.Features.ProcessKnmiMetarFiles;

public class ProcessKnmiMetarFilesFeature : IProcessKnmiMetarFilesFeature
{
    private readonly DutchMetarContext _dbContext;
    private readonly ILogger<ProcessKnmiMetarFilesFeature> _logger;
    private readonly IKnmiMetarFileClient _knmiFileClient;
    private readonly IMetarMapper _metarMapper;
    
    private const int BatchSize = 1000;
    private const string MetarStringRegex = @"<!--\s*(METAR.*?)\s*-->";
    private const string AirportNameRegex = @"<aixm:name>(.*?)<\/aixm:name>";
    private const string AirportIcaoRegex = @"<aixm:locationIndicatorICAO>(.*?)<\/aixm:locationIndicatorICAO>";

    public ProcessKnmiMetarFilesFeature(DutchMetarContext dbContext, ILogger<ProcessKnmiMetarFilesFeature> logger, IKnmiMetarFileClient knmiFileClient, IMetarMapper metarMapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _knmiFileClient = knmiFileClient;
        _metarMapper = metarMapper;
    }

    public async Task ProcessMetarFileBatchAsync(CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Processing KNMI files batch");
        var parser = new MetarParser();
        var files = await _dbContext.KnmiMetarFiles
            .Where(x => !x.IsFileProcessed)
            .OrderByDescending(x => x.CreatedAt)
            .Take(BatchSize)
            .ToArrayAsync(cancellationToken);

        foreach (var file in files)
        {
            var fileContent = await _knmiFileClient.GetKnmiMetarFileContentAsync(file.FileName, cancellationToken);
            
            var metarStringMatch = Regex.Match(fileContent, MetarStringRegex);
            var nameStringMatch = Regex.Match(fileContent, AirportNameRegex);

            if (!metarStringMatch.Success)
            {
                _logger.LogWarning($"Failed to extract METAR from file {file.FileName}.");
                continue;
            }
            
            Metar decodedMetar;

            try
            {
                decodedMetar = parser.Parse(metarStringMatch.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse {Metar}", metarStringMatch.Value);
                continue;
            }
            
            if (string.IsNullOrWhiteSpace(decodedMetar?.Airport)) continue;
            var airport = await GetAirportIncludingLatestMetarAsync(decodedMetar.Airport, correlationId, cancellationToken);
            
            var mappedMetarEntity = _metarMapper.MapDecodedMetarToEntity(decodedMetar, metarStringMatch.Value, airport, correlationId);
            
            var latestSavedMetar = airport.MetarReports.FirstOrDefault();
            if (latestSavedMetar == null || latestSavedMetar?.IssuedAt < mappedMetarEntity.IssuedAt)
            {
                _dbContext.Metars.Add(mappedMetarEntity);
            }
            else if (latestSavedMetar != null && latestSavedMetar.IssuedAt.Date == mappedMetarEntity.IssuedAt.Date &&
                     mappedMetarEntity.IsCorrected)
            {
                // METAR is a correction to previous issued one, so we update existing record 
                _dbContext.Metars.Remove(latestSavedMetar);
                _dbContext.Metars.Add(mappedMetarEntity);
            }
            
            file.IsFileProcessed = true;
            await  _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
    
    private async Task<Airport> GetAirportIncludingLatestMetarAsync(string icao, Guid correlationId, CancellationToken cancellationToken)
    {
        var airport = await _dbContext.Airports.FirstOrDefaultAsync(x => x.Icao == icao.ToUpperInvariant(), cancellationToken: cancellationToken);

        if (airport == null)
        {
            airport = new Airport
            {
                Icao = icao,
                MetarReports = [],
                CorrelationId = correlationId
            };

            _dbContext.Airports.Add(airport);
            return airport;
        }
        
        // Get the latest issued METAR for this airport
        var latestSavedMetar = await _dbContext.Metars
            .Where(x => x.AirportId == airport.Id)
            .OrderByDescending(x => x.IssuedAt)
            .Take(1)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        airport.MetarReports = latestSavedMetar != null ? [latestSavedMetar] : [];
        return airport;
    }
}