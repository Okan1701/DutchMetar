using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Enums;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Tools.StubService.Services;

public class MetarStubDataService : IMetarStubDataService
{
    private readonly DutchMetarContext _context;
    private readonly ILogger<MetarStubDataService> _logger;

    public MetarStubDataService(DutchMetarContext context, ILogger<MetarStubDataService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task GenerateMetarReportForAllAirportsAsync(CancellationToken cancellationToken)
    {
        var allAirports = await _context.Airports
            .OrderBy(x => x.Icao)
            .Take(100)
            .ToArrayAsync(cancellationToken);

        if (allAirports.Length == 0)
        {
            _logger.LogInformation("No Airports found. Skipping METAR generation.");
            return;
        }

        foreach (var airport in allAirports)
        {
            var metar = GenerateMetarForAirport(airport);
            _context.Metars.Add(metar);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static Metar GenerateMetarForAirport(Airport airport)
    {
        var metar = new Metar
        {
            AirportId = airport.Id,
            Airport = airport,
            RawMetar = string.Empty,
            IssuedAt = DateTimeOffset.UtcNow.AddSeconds(-DateTime.UtcNow.Second).AddMilliseconds(-DateTime.UtcNow.Millisecond),
            IsAuto = false,
            IsCavok = false,
            IsCorrected = false,
            NoCloudsDetected = false,
            TrendType = TrendType.None,
            Ceilings = []
        };

        var rng = new Random(airport.Id ^ DateTime.UtcNow.Millisecond);
        var issuedAt = metar.IssuedAt;

        var windDirection = rng.Next(0, 360);
        var windSpeed = rng.Next(0, 40);
        int? windGust = null;
        if (windSpeed > 10 && rng.NextDouble() < 0.3)
        {
            windGust = windSpeed + rng.Next(5, 20);
        }

        metar.WindDirection = windDirection;
        metar.WindSpeedKnots = windSpeed;
        metar.WindSpeedGustsKnots = windGust;

        var visibilityMeters = rng.NextDouble() < 0.8 ? 9999 : rng.Next(100, 9000);
        metar.VisibilityMeters = visibilityMeters;

        var temperature = rng.Next(-10, 35);
        var dewpoint = temperature - rng.Next(0, 6);
        metar.TemperatureCelsius = temperature;
        metar.DewpointCelsius = dewpoint;

        metar.AltimeterValue = rng.Next(980, 1045);
        metar.IsAuto = rng.NextDouble() < 0.05;
        metar.IsCorrected = rng.NextDouble() < 0.02;

        var trendRoll = rng.NextDouble();
        if (trendRoll < 0.8)
        {
            metar.TrendType = TrendType.None;
        }
        else if (trendRoll < 0.9)
        {
            metar.TrendType = TrendType.Nosig;
        }
        else if (trendRoll < 0.95)
        {
            metar.TrendType = TrendType.Tempo;
        }
        else
        {
            metar.TrendType = TrendType.Becmg;
        }

        if (rng.NextDouble() < 0.25)
        {
            metar.Remarks = "NOSIG";
        }

        var ceilingCount = rng.Next(1, 7);
        var ceilings = new List<MetarCeiling>(ceilingCount);
        var cloudGroups = new List<string>(ceilingCount);
        var usedHeights = new HashSet<int>();

        for (var i = 0; i < ceilingCount; i++)
        {
            var type = (CeilingType)rng.Next(1, 5);
            var heightHundreds = 0;
            var attempts = 0;

            do
            {
                heightHundreds = rng.Next(3, 60);
                attempts++;
            }
            while (usedHeights.Contains(heightHundreds) && attempts < 10);

            usedHeights.Add(heightHundreds);

            var ceiling = new MetarCeiling
            {
                Type = type,
                Height = heightHundreds * 100,
                Metar = metar
            };

            ceilings.Add(ceiling);

            var code = type switch
            {
                CeilingType.Few => "FEW",
                CeilingType.Scattered => "SCT",
                CeilingType.Broken => "BKN",
                CeilingType.Overcast => "OVC",
                _ => "FEW"
            };

            cloudGroups.Add($"{code}{heightHundreds:D3}");
        }

        metar.Ceilings = ceilings;

        var icao = airport.Icao.ToUpperInvariant();
        var dayHour = issuedAt.UtcDateTime.ToString("ddHHmm");

        string windString;
        if (windSpeed == 0)
        {
            windString = "00000KT";
        }
        else
        {
            var direction = windDirection.ToString("D3");
            windString = windGust.HasValue
                ? $"{direction}{windSpeed:D2}G{windGust.Value:D2}KT"
                : $"{direction}{windSpeed:D2}KT";
        }

        var visibilityString = visibilityMeters >= 9999 ? "9999" : visibilityMeters.ToString();

        string cloudsString;
        if (visibilityMeters >= 10000 && ceilings.Count == 0)
        {
            metar.IsCavok = true;
            cloudsString = "CAVOK";
        }
        else
        {
            metar.IsCavok = false;
            cloudsString = string.Join(" ", cloudGroups);
        }

        var tempValue = metar.TemperatureCelsius ?? 0;
        var dewpointValue = metar.DewpointCelsius ?? 0;
        var temperatureString = (tempValue < 0 ? "M" + Math.Abs(tempValue) : tempValue.ToString()) + "/" +
            (dewpointValue < 0 ? "M" + Math.Abs(dewpointValue) : dewpointValue.ToString());

        var qnhString = $"Q{metar.AltimeterValue:D4}";

        var trendString = metar.TrendType switch
        {
            TrendType.None => string.Empty,
            TrendType.Nosig => " NOSIG",
            TrendType.Tempo => " TEMPO",
            TrendType.Becmg => " BECMG",
            _ => string.Empty
        };

        var remarks = string.IsNullOrWhiteSpace(metar.Remarks) ? string.Empty : " " + metar.Remarks;

        var rawParts = new List<string> { icao, dayHour + "Z", windString, visibilityString };
        if (!string.IsNullOrWhiteSpace(cloudsString))
        {
            rawParts.Add(cloudsString);
        }

        rawParts.Add(temperatureString);
        rawParts.Add(qnhString);

        if (!string.IsNullOrWhiteSpace(trendString))
        {
            rawParts.Add(trendString.Trim());
        }

        if (!string.IsNullOrWhiteSpace(remarks))
        {
            rawParts.Add(metar.Remarks!);
        }

        metar.RawMetar = string.Join(" ", rawParts);
        return metar;
    }
}