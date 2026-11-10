using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Exceptions;
using DutchMetar.Core.Features.Web.AirportPerDayHistory;
using DutchMetar.Core.Features.Web.AirportPerDayHistory.Models;

namespace DutchMetar.Core.Tests.Features.Web.AirportPerDayHistory;

public class GetAirportDayHistoryFeatureTests : TestsWithContext
{
    [Fact]
    public async Task GetAirportDayHistoryAsync_WithLowercaseIcaoAndReportsInDateRange_ReturnsSortedHistory()
    {
        var airport = new Airport { Icao = "EHAM", Name = "Schiphol" };
        var date = new DateOnly(2026, 10, 4);
        var startOfDay = new DateTimeOffset(2026, 10, 4, 0, 0, 0, TimeSpan.Zero);
        var firstInRange = startOfDay.AddHours(10);
        var lastInRange = startOfDay.AddHours(11);
        Context.Airports.Add(airport);
        Context.Metars.AddRange(
            CreateMetar(airport, startOfDay.AddDays(-1), "BEFORE"),
            CreateMetar(airport, firstInRange, "FIRST", 14, 8, 10000, 10),
            CreateMetar(airport, lastInRange, "LAST", 16, 9, 9000, 12),
            CreateMetar(airport, startOfDay.AddDays(2), "AFTER"));
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await new GetAirportDayHistoryFeature(Context).GetAirportDayHistoryAsync(
            new GetAirportDayHistoryInput { Icao = "eham", Date = date });

        Assert.Equal("EHAM", result.Icao);
        Assert.True(result.IsMissingData);
        Assert.Equal(new[] { lastInRange, firstInRange }, result.History.Select(x => x.DateTime));
        Assert.Collection(
            result.History,
            latest =>
            {
                Assert.Equal(16, latest.TemperatureCelsius);
                Assert.Equal(9, latest.DewpointCelsius);
                Assert.Equal(9000, latest.VisibilityMeters);
                Assert.Equal(12, latest.WindSpeedKnots);
            },
            earlier =>
            {
                Assert.Equal(14, earlier.TemperatureCelsius);
                Assert.Equal(8, earlier.DewpointCelsius);
                Assert.Equal(10000, earlier.VisibilityMeters);
                Assert.Equal(10, earlier.WindSpeedKnots);
            });
    }

    [Fact]
    public async Task GetAirportDayHistoryAsync_WithUnknownIcao_ThrowsEntityNotFoundException()
    {
        var feature = new GetAirportDayHistoryFeature(Context);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => feature.GetAirportDayHistoryAsync(
            new GetAirportDayHistoryInput { Icao = "XXXX", Date = new DateOnly(2026, 10, 4) }));
    }

    [Fact]
    public async Task GetAirportDayHistoryAsync_WithoutReports_ReturnsEmptyHistoryWithMissingData()
    {
        Context.Airports.Add(new Airport { Icao = "EHAM", Name = "Schiphol" });
        await Context.SaveChangesAsync();

        var result = await new GetAirportDayHistoryFeature(Context).GetAirportDayHistoryAsync(
            new GetAirportDayHistoryInput { Icao = "EHAM", Date = new DateOnly(2026, 10, 4) });

        Assert.True(result.IsMissingData);
        Assert.Empty(result.History);
    }

    [Fact]
    public async Task GetAirportDayHistoryAsync_WithCompleteReportsInFinalHour_ReturnsNoMissingData()
    {
        var airport = new Airport { Icao = "EHAM", Name = "Schiphol" };
        Context.Airports.Add(airport);
        Context.Metars.AddRange(
            CreateMetar(airport, new DateTimeOffset(2026, 10, 4, 23, 10, 0, TimeSpan.Zero), "FIRST", 15, 10, 10000, 10),
            CreateMetar(airport, new DateTimeOffset(2026, 10, 4, 23, 45, 0, TimeSpan.Zero), "LAST", 16, 11, 10000, 12));
        await Context.SaveChangesAsync();

        var result = await new GetAirportDayHistoryFeature(Context).GetAirportDayHistoryAsync(
            new GetAirportDayHistoryInput { Icao = "EHAM", Date = new DateOnly(2026, 10, 4) });

        Assert.False(result.IsMissingData);
        Assert.Equal(2, result.History.Count);
    }

    [Fact]
    public async Task GetAirportDayHistoryAsync_WithOneHourGapBetweenReports_ReturnsMissingData()
    {
        var airport = new Airport { Icao = "EHAM", Name = "Schiphol" };
        Context.Airports.Add(airport);
        Context.Metars.AddRange(
            CreateMetar(airport, new DateTimeOffset(2026, 10, 4, 22, 30, 0, TimeSpan.Zero), "EARLIER", 15, 10, 10000, 10),
            CreateMetar(airport, new DateTimeOffset(2026, 10, 4, 23, 30, 0, TimeSpan.Zero), "LATEST", 16, 11, 10000, 12));
        await Context.SaveChangesAsync();

        var result = await new GetAirportDayHistoryFeature(Context).GetAirportDayHistoryAsync(
            new GetAirportDayHistoryInput { Icao = "EHAM", Date = new DateOnly(2026, 10, 4) });

        Assert.True(result.IsMissingData);
    }

    [Fact]
    public async Task GetAirportDayHistoryAsync_WithNullWeatherValue_ReturnsMissingData()
    {
        var airport = new Airport { Icao = "EHAM", Name = "Schiphol" };
        Context.Airports.Add(airport);
        Context.Metars.AddRange(
            CreateMetar(airport, new DateTimeOffset(2026, 10, 4, 23, 10, 0, TimeSpan.Zero), "EARLIER", 15, 10, 10000, 10),
            CreateMetar(airport, new DateTimeOffset(2026, 10, 4, 23, 45, 0, TimeSpan.Zero), "LATEST", null, 11, 10000, 12));
        await Context.SaveChangesAsync();

        var result = await new GetAirportDayHistoryFeature(Context).GetAirportDayHistoryAsync(
            new GetAirportDayHistoryInput { Icao = "EHAM", Date = new DateOnly(2026, 10, 4) });

        Assert.True(result.IsMissingData);
    }

    private static Metar CreateMetar(
        Airport airport,
        DateTimeOffset issuedAt,
        string rawMetar,
        int? temperature = null,
        int? dewpoint = null,
        int? visibility = null,
        int? windSpeed = null)
    {
        return new Metar
        {
            Airport = airport,
            RawMetar = rawMetar,
            IssuedAt = issuedAt,
            TemperatureCelsius = temperature,
            DewpointCelsius = dewpoint,
            VisibilityMeters = visibility,
            WindSpeedKnots = windSpeed
        };
    }
}
