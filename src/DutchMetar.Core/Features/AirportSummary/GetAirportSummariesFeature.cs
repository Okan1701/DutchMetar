using DutchMetar.Core.Features.AirportSummary.Interfaces;
using DutchMetar.Core.Features.AirportSummary.Models;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Core.Features.AirportSummary;

public class GetAirportSummariesFeature : IGetAirportSummariesFeature
{
    private readonly DutchMetarContext _dbContext;

    public GetAirportSummariesFeature(DutchMetarContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<SingleAirportSummary>> GetAirportSummariesAsync()
    {
        var allAirports = await _dbContext.Airports
            .Take(100)
            .Include(x => x.MetarReports
                .OrderByDescending(m => m.IssuedAt)
                .Take(1)
            )
            .Where(x => x.MetarReports.Any())
            .ToArrayAsync();

        return allAirports.Select(x => new SingleAirportSummary
        {
            Icao = x.Icao,
            IsAuto = x.MetarReports.First().IsAuto,
            IsCavok = x.MetarReports.First().IsCavok,
            LastIssuedMetarDate = x.MetarReports.First().IssuedAt,
            WindDirection = x.MetarReports.First().WindDirection,
            WindSpeedKnots = x.MetarReports.First().WindSpeedKnots
        }).ToArray();
    }
}