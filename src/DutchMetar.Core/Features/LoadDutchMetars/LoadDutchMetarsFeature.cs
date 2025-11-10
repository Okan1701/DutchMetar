using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.LoadDutchMetars.Exceptions;
using DutchMetar.Core.Features.LoadDutchMetars.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using MetarParserCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Metar = MetarParserCore.Objects.Metar;

namespace DutchMetar.Core.Features.LoadDutchMetars;

public class LoadDutchMetarsFeature : ILoadDutchMetarsFeature
{
    private readonly IKnmiMetarRepository _repository;
    private readonly IMetarMapper _metarMapper;
    private readonly DutchMetarContext _context;
    private readonly ILogger<LoadDutchMetarsFeature> _logger;

    public LoadDutchMetarsFeature(IKnmiMetarRepository repository, DutchMetarContext context, ILogger<LoadDutchMetarsFeature> logger, IMetarMapper metarMapper)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
        _metarMapper = metarMapper;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving METARs from KNMI");
        var correlationId = Guid.NewGuid();
        var metars = await _repository.GetKnmiRawMetarsAsync();
        var parser = new MetarParser();
        
        foreach (var metar in metars)
        {
            Metar decodedMetar;

            try
            {
                decodedMetar = parser.Parse(metar);
            }
            catch (Exception ex)
            {
                throw new MetarParseException($"Failed to parse {metar}", ex);
            }

            if (string.IsNullOrWhiteSpace(decodedMetar?.Airport)) continue;
            
            var airport = await GetAirportIncludingLatestMetarAsync(decodedMetar.Airport, correlationId, cancellationToken);
            var mappedMetarEntity = _metarMapper.MapDecodedMetarToEntity(decodedMetar, metar, airport, correlationId);
            
            var latestSavedMetar = airport.MetarReports.FirstOrDefault();
            if (latestSavedMetar == null || latestSavedMetar?.IssuedAt < mappedMetarEntity.IssuedAt)
            {
                _context.Metars.Add(mappedMetarEntity);
            }
            else if (latestSavedMetar != null && latestSavedMetar.IssuedAt.Date == mappedMetarEntity.IssuedAt.Date &&
                mappedMetarEntity.IsCorrected)
            {
                // METAR is a correction to previous issued one, so we update existing record 
                _context.Metars.Remove(latestSavedMetar);
                _context.Metars.Add(mappedMetarEntity);
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    private async Task<Airport> GetAirportIncludingLatestMetarAsync(string icao, Guid correlationId, CancellationToken cancellationToken)
    {
        var airport = await _context.Airports.FirstOrDefaultAsync(x => x.Icao == icao.ToUpperInvariant(), cancellationToken: cancellationToken);

        if (airport == null)
        {
            airport = new Airport
            {
                Icao = icao,
                MetarReports = [],
                CorrelationId = correlationId
            };

            _context.Airports.Add(airport);
            return airport;
        }
        
        // Get the latest issued METAR for this airport
        var latestSavedMetar = await _context.Metars
            .Where(x => x.AirportId == airport.Id)
            .OrderByDescending(x => x.IssuedAt)
            .Take(1)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        airport.MetarReports = latestSavedMetar != null ? [latestSavedMetar] : [];
        return airport;
    }
}