using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Enums;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Tools.StubData;

public class StubDataGenerator
{
    private readonly DutchMetarContext _context;
    private DateTimeOffset InstantiatedDateUtc => DateTimeOffset.UtcNow;
    private const string StubAirportIcao = "EHXX";

    public StubDataGenerator(DutchMetarContext context)
    {
        _context = context;
    }

    public async Task GeneratePeriodicMetarsAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Generating periodic metars.");
        var stubAirport = await GetOrCreateStubAirportAsync(cancellationToken);
        
        // Clear existing metar reports
        await _context.Metars
            .Where(x => x.AirportId == stubAirport.Id)
            .ExecuteDeleteAsync(cancellationToken);
        
        var generatedCount = 1;
        while (!cancellationToken.IsCancellationRequested)
        {
            await GeneratePeriodMetarsForAirportAsync(stubAirport, generatedCount, cancellationToken);
            Console.WriteLine($"New METAR #{generatedCount} added to airport {stubAirport.Icao}.");
            await Task.Delay(1000, cancellationToken);
            generatedCount++;
        }
    }

    private async Task GeneratePeriodMetarsForAirportAsync(Airport airport, int currentGeneratedCount,
        CancellationToken cancellationToken)
    {
        var random = new Random();
        var issuedAt = DateTime.UtcNow.AddMinutes(-30 * (currentGeneratedCount - 1));

        // Generate random METAR values
        var isAuto = random.Next(0, 2) == 1;
        var isCavok = random.Next(0, 2) == 1;
        var isCorrected = random.Next(0, 2) == 1;
        var windDirection = random.Next(0, 361);
        var windSpeedKnots = random.Next(0, 50);
        int? windSpeedGustsKnots = random.Next(0, 2) == 1 ? random.Next(windSpeedKnots, 70) : null;
        var noCloudsDetected = random.Next(0, 2) == 1;
        var visibilityMeters = random.Next(50, 10000);
        var temperatureCelsius = random.Next(-20, 40);
        var dewpointCelsius = random.Next(-20, temperatureCelsius);
        var altimeterValue = random.Next(950, 1050);
        var remarks = random.Next(0, 2) == 1 ? "REMARKS" : null;

        // Generate a raw METAR string (simplified for stub)
        var rawMetar = $"METAR {airport.Icao} {issuedAt:yyyyMMddHHmm}Z AUTO {windDirection:D3}/{windSpeedKnots:D2}G{windSpeedGustsKnots ?? 0:D2}KT {visibilityMeters:D4} {temperatureCelsius:D2}/{dewpointCelsius:D2} Q{altimeterValue:D4}";

        // Create METAR
        var metar = new Metar
        {
            AirportId = airport.Id,
            IsAuto = isAuto,
            IsCavok = isCavok,
            IsCorrected = isCorrected,
            WindDirection = windDirection,
            WindSpeedKnots = windSpeedKnots,
            WindSpeedGustsKnots = windSpeedGustsKnots,
            NoCloudsDetected = noCloudsDetected,
            VisibilityMeters = visibilityMeters,
            RawMetar = rawMetar,
            TemperatureCelsius = temperatureCelsius,
            DewpointCelsius = dewpointCelsius,
            AltimeterValue = altimeterValue,
            Remarks = remarks,
            IssuedAt = issuedAt,
            Ceilings = new List<MetarCeiling>()
        };

        // Add a random ceiling if not CAVOK and not no clouds detected
        if (!isCavok && !noCloudsDetected && random.Next(0, 2) == 1)
        {
            var ceilingTypes = Enum.GetValues(typeof(CeilingType)).Cast<CeilingType>().ToList();
            var ceilingType = ceilingTypes[random.Next(ceilingTypes.Count)];
            var height = random.Next(100, 10000);

            metar.Ceilings.Add(new MetarCeiling
            {
                Type = ceilingType,
                Height = height
            });
        }

        // Save to database
        _context.Metars.Add(metar);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Airport> GetOrCreateStubAirportAsync(CancellationToken cancellationToken)
    {
        var airport = await _context.Airports.FirstOrDefaultAsync(x => x.Icao == StubAirportIcao, cancellationToken: cancellationToken);

        if (airport == null)
        {
            airport = new Airport
            {
                Icao = StubAirportIcao,
                Name = "Noord-Zee Airport NLD (STUB)"
            };
            _context.Airports.Add(airport);
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        return airport;
    }
}