using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Exceptions;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using MetarParserCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Metar = MetarParserCore.Objects.Metar;

namespace DutchMetar.Core.Features.SyncKnmiMetarFileList;

public class MetarProcessor : IMetarProcessor
{
    private readonly IMetarMapper _metarMapper;
    private readonly ILogger<MetarProcessor>  _logger;
    private readonly DutchMetarContext _dbContext;
    private readonly MetarParser _parser = new();
    
    public MetarProcessor(IMetarMapper metarMapper, ILogger<MetarProcessor> logger, DutchMetarContext dbContext)
    {
        _metarMapper = metarMapper;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task ProcessRawMetarAsync(string metar, string? airportName, CancellationToken cancellationToken)
    {
        Metar decodedMetar;
        try
        {
            decodedMetar = _parser.Parse(metar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse {Metar}", metar);
            throw new MetarParseException($"Failed to parse {metar}", ex);
        }

        if (string.IsNullOrWhiteSpace(decodedMetar?.Airport))
        {
            throw new MetarParseException($"Failed to parse {metar}");
        };
        
        var airport = await GetAirportIncludingLatestMetarAsync(decodedMetar.Airport, cancellationToken);
        airport.Name = airportName ?? airport.Icao;
        var mappedMetarEntity = _metarMapper.MapDecodedMetarToEntity(decodedMetar, metar, airport);
        
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
    }
    
    private async Task<Airport> GetAirportIncludingLatestMetarAsync(string icao, CancellationToken cancellationToken)
    {
        var airport = await _dbContext.Airports.FirstOrDefaultAsync(x => x.Icao == icao.ToUpperInvariant(), cancellationToken: cancellationToken);

        if (airport == null)
        {
            airport = new Airport
            {
                Icao = icao,
                MetarReports = []
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