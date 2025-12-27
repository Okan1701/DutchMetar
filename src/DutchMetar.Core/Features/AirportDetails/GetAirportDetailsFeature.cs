using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Exceptions;
using DutchMetar.Core.Features.AIrportDetails.Interfaces;
using DutchMetar.Core.Features.AirportDetails.Models;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Core.Features.AirportDetails;

public class GetAirportDetailsFeature : IGetAirportDetailsFeature
{
    private readonly DutchMetarContext _dbContext;

    public GetAirportDetailsFeature(DutchMetarContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Models.AirportDetails> GetAirportDetailsAsync(string airportCode, CancellationToken cancellationToken = default)
    {
        var last24Hours = DateTime.UtcNow.AddHours(-24);
        
        var airport = await _dbContext.Airports
            .Include(x => x.MetarReports
                .OrderByDescending(m => m.IssuedAt)
                .Where(m => m.IssuedAt >=  last24Hours)
                .Take(1))
            .FirstOrDefaultAsync(x => x.Icao.ToUpper() == airportCode.ToUpper(), cancellationToken: cancellationToken);

        if (airport == null)
        {
            throw new EntityNotFoundException(nameof(Airport),  airportCode);
        }

        var airportDetails = new Models.AirportDetails
        {
            Icao = airport.Icao,
        };

        var latestMetar = airport.MetarReports.FirstOrDefault();
        if (latestMetar != null)
        {
            airportDetails.CurrentWeather = MapMetarEntityToModel(latestMetar);
            airportDetails.LastUpdated = latestMetar.LastUpdatedAt;
        }
        
        airportDetails.HistoricalWeather = airport.MetarReports
            .Select(MapMetarEntityToModel)
            .ToArray();

        return airportDetails;
    }

    private AirportCurrentMetar MapMetarEntityToModel(Metar metar) => new()
    {
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
    };
}