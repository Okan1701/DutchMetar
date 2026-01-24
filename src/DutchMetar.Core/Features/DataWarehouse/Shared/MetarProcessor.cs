using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using DutchMetar.Core.Infrastructure.Accessors;
using DutchMetar.Core.Infrastructure.Data;
using MetarParserCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Metar = MetarParserCore.Objects.Metar;

namespace DutchMetar.Core.Features.DataWarehouse.Shared;

public class MetarProcessor : IMetarProcessor
{
    private readonly IMetarMapper _metarMapper;
    private readonly ILogger<MetarProcessor>  _logger;
    private readonly DutchMetarContext _dbContext;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;
    private readonly MetarParser _parser = new();
    
    public MetarProcessor(IMetarMapper metarMapper, ILogger<MetarProcessor> logger, DutchMetarContext dbContext, ICorrelationIdAccessor correlationIdAccessor)
    {
        _metarMapper = metarMapper;
        _logger = logger;
        _dbContext = dbContext;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public async Task ProcessRawMetarAsync(string metar, string? airportName, DateTimeOffset createdAt, CancellationToken cancellationToken)
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
        
        var airport = await GetAirportIncludingLatestMetarAsync(decodedMetar.Airport, cancellationToken, _correlationIdAccessor.CorrelationId);
        airport.Name = airportName ?? airport.Icao;
        var mappedMetarEntity = _metarMapper.MapDecodedMetarToEntity(decodedMetar, metar, createdAt, airport);
        
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
        else _logger.LogDebug("Value {Metar} not saved. It is not newer than the latest saved METAR and is not a correction.", metar);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task<Airport> GetAirportIncludingLatestMetarAsync(string icao, CancellationToken cancellationToken, Guid correlationId)
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