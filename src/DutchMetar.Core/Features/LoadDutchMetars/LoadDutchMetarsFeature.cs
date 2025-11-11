using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Enums;
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
        var parser = new MetarParser();
        var importResult = new MetarImportResult();
        var correlationId = Guid.NewGuid();
        var failedMetarParses = new List<string>();
        importResult.CorrelationId = correlationId;
        _context.MetarImportResults.Add(importResult);

        try
        {
            var metars = await _repository.GetKnmiRawMetarsAsync();
            importResult.IsSuccess = true;
            importResult.Result = ImportResult.Succeeded;
            foreach (var metar in metars)
            {
                Metar decodedMetar;

                try
                {
                    decodedMetar = parser.Parse(metar);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse {Metar}", metar);
                    importResult.Result = ImportResult.SucceededWithErrors;
                    failedMetarParses.Add(metar);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(decodedMetar?.Airport)) continue;
            
                var airport = await GetAirportIncludingLatestMetarAsync(decodedMetar.Airport, correlationId, cancellationToken);
                if (airport.Id == 0) importResult.AddedAirportCount++;
                
                var mappedMetarEntity = _metarMapper.MapDecodedMetarToEntity(decodedMetar, metar, airport, correlationId);
            
                var latestSavedMetar = airport.MetarReports.FirstOrDefault();
                if (latestSavedMetar == null || latestSavedMetar?.IssuedAt < mappedMetarEntity.IssuedAt)
                {
                    _context.Metars.Add(mappedMetarEntity);
                    importResult.AddedMetarCount++;
                }
                else if (latestSavedMetar != null && latestSavedMetar.IssuedAt.Date == mappedMetarEntity.IssuedAt.Date &&
                         mappedMetarEntity.IsCorrected)
                {
                    // METAR is a correction to previous issued one, so we update existing record 
                    _context.Metars.Remove(latestSavedMetar);
                    _context.Metars.Add(mappedMetarEntity);
                    importResult.CorrectedMetarCount++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "METAR Import failed");
            importResult.IsSuccess = false;
            importResult.Result = ImportResult.Failed;
            importResult.ExceptionName = ex.GetType().Name;
            importResult.ExceptionMessage = ex.Message;
            importResult.ExceptionTrace = ex.StackTrace;
            await _context.SaveChangesAsync(cancellationToken);
            throw;
        }

        if (failedMetarParses.Count > 0) importResult.FailedMetarParses = string.Join(";", failedMetarParses);
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