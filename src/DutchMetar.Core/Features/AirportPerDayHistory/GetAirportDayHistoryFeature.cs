using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Exceptions;
using DutchMetar.Core.Features.AirportPerDayHistory.Interfaces;
using DutchMetar.Core.Features.AirportPerDayHistory.Models;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Core.Features.AirportPerDayHistory;

public class GetAirportDayHistoryFeature : IGetAirportDayHistoryFeature
{
    private readonly DutchMetarContext  _context;

    public GetAirportDayHistoryFeature(DutchMetarContext context)
    {
        _context = context;
    }

    public async Task<AirportDayHistory> GetAirportDayHistoryAsync(GetAirportDayHistoryInput input, CancellationToken cancellationToken = default)
    {
        var airportData = await GetAirportWithMetar(input, cancellationToken);
        var sortedMetarData = airportData.MetarReports.OrderByDescending(x => x.IssuedAt).ToArray();

        return new()
        {
            Icao = airportData.Icao,
            IsMissingData = HasMissingData(sortedMetarData),
            History = sortedMetarData.Select(x => new AirportDayHistorySnapshot
            {
                DateTime = x.IssuedAt,
                TemperatureCelsius = x.TemperatureCelsius,
                DewpointCelsius = x.DewpointCelsius,
                VisibilityMeters = x.VisibilityMeters,
                WindSpeedKnots = x.WindSpeedKnots
            }).ToArray()
        };
    }
    
    /// <summary>
    /// Retrieves specific airport with all associated METAR reports within target day.
    /// </summary>
    /// <exception cref="EntityNotFoundException">Airport with specified ICAO does not exist.</exception>
    private async Task<Airport> GetAirportWithMetar(GetAirportDayHistoryInput input,
        CancellationToken cancellationToken = default)
    {
        var start = input.Date.ToDateTime(TimeOnly.MinValue);
        var end = input.Date.ToDateTime(TimeOnly.MaxValue);
        var normalizedIcao = input.Icao.ToUpperInvariant();

        var results = await _context.Airports
            .Where(x => x.Icao == normalizedIcao)
            .Include(x => x.MetarReports
                .Where(y => y.IssuedAt >= start && y.IssuedAt <= end))
            .FirstOrDefaultAsync(cancellationToken);

        return results ?? throw new EntityNotFoundException(nameof(Airport), normalizedIcao);
    }
    
    /// <summary>
    /// Checks is there is a data gap in the provided <param name="sortedMetarData"></param>.
    /// A timegap of >= 1 hour between METAR reports, or any null value in a metar report is considered a gap
    /// </summary>
    /// <param name="sortedMetarData">Collection of <see cref="Metar"/> sorted DESC by IssuedAt property.</param>
    private bool HasMissingData(ICollection<Metar> sortedMetarData)
    {
        var isMissingData = false;

        if (sortedMetarData.Count == 0)
        {
            return true;
        }
        
        // Scan through the METAR reports to identify missing data
        Metar? previousScannedMetar = null;
        foreach (var metar in sortedMetarData)
        {
            // Check timegap with last metar.
            if (previousScannedMetar != null)
            {
                // Timegap of 1 hour or more is considered missing data
                bool isAtLeastOneHour = (metar.IssuedAt - previousScannedMetar.IssuedAt).Duration() >= TimeSpan.FromHours(1);
                if (isAtLeastOneHour)
                {
                    isMissingData = true;
                }
            }
            
            var hasNullProperty = metar.TemperatureCelsius == null ||
                                  metar.DewpointCelsius == null ||
                                  metar.VisibilityMeters == null ||
                                  metar.WindSpeedKnots == null;
            
            if (hasNullProperty)
            {
                isMissingData = true;
            }
            
            previousScannedMetar = metar;
        }
        
        // Check if the newest METAR is at the end of the day
        // Otherwise we have a data gap as a full day of metar reports should cover every hour.
        var latestMetar = sortedMetarData.Last();
        if (latestMetar.IssuedAt.Hour < 23)
        {
            isMissingData = true;
        }

        return isMissingData;
    }
}