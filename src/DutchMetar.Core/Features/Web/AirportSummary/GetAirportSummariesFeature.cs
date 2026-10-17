using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.Web.AirportDetails.Models;
using DutchMetar.Core.Features.Web.AirportSummary.Interfaces;
using DutchMetar.Core.Features.Web.AirportSummary.Models;
using DutchMetar.Core.Helpers;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Core.Features.Web.AirportSummary;

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
            .ThenInclude(x => x.Ceilings)
            .Include(x => x.TafReports
                .OrderByDescending(t => t.IssuedAt)
                .Take(1)
            )
            .Where(x => x.MetarReports.Any())
            .ToArrayAsync();

        return allAirports.Select(airport =>
        {
            var latestMetar = airport.MetarReports.First();
            var summary = new SingleAirportSummary
            {
                Icao = airport.Icao,
                Name = airport.Name,
                MeteoCondition = MeteoConditionsHelper.GetMeteoCondition(latestMetar),
                LatestWeather = MapMetarEntityToModel(latestMetar)
            };

            var latestTaf = airport.TafReports.FirstOrDefault();
            if (latestTaf != null)
            {
                summary.LatestForecast = MapTafEntityToModel(latestTaf);
            }

            summary.LastUpdated = latestMetar.LastUpdatedAt;
            return summary;
        }).ToArray();
    }

    private AirportCurrentMetar MapMetarEntityToModel(Metar metar) => new()
    {
        IssuedAt = metar.IssuedAt,
        IsAuto = metar.IsAuto,
        IsCavok = metar.IsCavok,
        IsCorrected = metar.IsCorrected,
        WindDirection = metar.WindDirection,
        WindSpeedKnots = metar.WindSpeedKnots,
        WindSpeedGustsKnots = metar.WindSpeedGustsKnots,
        NoCloudsDetected = metar.NoCloudsDetected,
        VisibilityMeters = metar.VisibilityMeters,
        RawMetar = metar.RawMetar,
        TemperatureCelsius = metar.TemperatureCelsius,
        DewpointCelsius = metar.DewpointCelsius,
        AltimeterValue = metar.AltimeterValue,
        Remarks = metar.Remarks,
        Ceilings = metar.Ceilings?.Select(ceiling => new AirportMetarCeiling
        {
            Type = ceiling.Type,
            Height = ceiling.Height
        }).ToArray() ?? []
    };

    private AirportForecast MapTafEntityToModel(Taf taf) => new()
    {
        RawTaf = taf.RawTaf,
        IssuedAt = taf.IssuedAt ?? DateTimeOffset.MinValue,
    };
}