using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Enums;
using DutchMetar.Core.Features.Web.AirportSummary;
using DutchMetar.Core.Helpers.Enums;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Core.Tests.Features.Web.AirportSummary;

public class GetAirportSummariesFeatureTests : TestsWithContext
{
    [Fact]
    public async Task GetAirportSummariesAsync_WithReports_ReturnsLatestMappedReports()
    {
        var airport = new Airport { Icao = "EHAM", Name = "Schiphol" };
        var olderIssuedAt = new DateTimeOffset(2026, 10, 4, 10, 0, 0, TimeSpan.Zero);
        var latestIssuedAt = olderIssuedAt.AddHours(1);
        var latestMetar = new Metar
        {
            Airport = airport,
            RawMetar = "LATEST METAR",
            IssuedAt = latestIssuedAt,
            LastUpdatedAt = latestIssuedAt.AddMinutes(2),
            IsAuto = true,
            IsCavok = false,
            IsCorrected = true,
            WindDirection = 240,
            WindSpeedKnots = 12,
            WindSpeedGustsKnots = 20,
            NoCloudsDetected = false,
            VisibilityMeters = 6000,
            TemperatureCelsius = 15,
            DewpointCelsius = 10,
            AltimeterValue = 1013,
            Remarks = "remark",
            Ceilings =
            [
                new MetarCeiling { Type = CeilingType.Overcast, Height = 900 },
                new MetarCeiling { Type = CeilingType.Few, Height = 300 }
            ]
        };
        Context.Airports.Add(airport);
        Context.Metars.AddRange(
            new Metar
            {
                Airport = airport,
                RawMetar = "OLDER METAR",
                IssuedAt = olderIssuedAt
            },
            latestMetar);
        Context.Tafs.AddRange(
            new Taf
            {
                Airport = airport,
                RawTaf = "OLDER TAF",
                IssuedAt = olderIssuedAt
            },
            new Taf
            {
                Airport = airport,
                RawTaf = "LATEST TAF",
                IssuedAt = latestIssuedAt
            });
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await new GetAirportSummariesFeature(Context).GetAirportSummariesAsync();

        var summary = Assert.Single(result);
        Assert.Equal("EHAM", summary.Icao);
        Assert.Equal("Schiphol", summary.Name);
        Assert.Equal(latestMetar.LastUpdatedAt, summary.LastUpdated);
        Assert.Equal(MetarMeteoCondition.Imc, summary.MeteoCondition);
        Assert.NotNull(summary.LatestWeather);
        Assert.Equal("LATEST METAR", summary.LatestWeather.RawMetar);
        Assert.Equal(latestIssuedAt, summary.LatestWeather.IssuedAt);
        Assert.True(summary.LatestWeather.IsAuto);
        Assert.False(summary.LatestWeather.IsCavok);
        Assert.True(summary.LatestWeather.IsCorrected);
        Assert.False(summary.LatestWeather.NoCloudsDetected);
        Assert.Equal(240, summary.LatestWeather.WindDirection);
        Assert.Equal(12, summary.LatestWeather.WindSpeedKnots);
        Assert.Equal(20, summary.LatestWeather.WindSpeedGustsKnots);
        Assert.Equal(6000, summary.LatestWeather.VisibilityMeters);
        Assert.Equal(15, summary.LatestWeather.TemperatureCelsius);
        Assert.Equal(10, summary.LatestWeather.DewpointCelsius);
        Assert.Equal(1013, summary.LatestWeather.AltimeterValue);
        Assert.Equal("remark", summary.LatestWeather.Remarks);
        Assert.Equal(
            new[] { (CeilingType.Overcast, 900), (CeilingType.Few, 300) },
            summary.LatestWeather.Ceilings.Select(x => (x.Type, x.Height)));
        Assert.NotNull(summary.LatestForecast);
        Assert.Equal("LATEST TAF", summary.LatestForecast.RawTaf);
        Assert.Equal(latestIssuedAt, summary.LatestForecast.IssuedAt);
    }

    [Fact]
    public async Task GetAirportSummariesAsync_WithoutMetars_ExcludesAirportAndLeavesForecastNull()
    {
        var airportWithMetar = new Airport { Icao = "EHAM", Name = "Schiphol" };
        var airportWithoutMetar = new Airport { Icao = "EHRD", Name = "Rotterdam" };
        Context.Airports.AddRange(airportWithMetar, airportWithoutMetar);
        Context.Metars.Add(new Metar
        {
            Airport = airportWithMetar,
            RawMetar = "METAR",
            IssuedAt = DateTimeOffset.UtcNow
        });
        Context.Tafs.Add(new Taf
        {
            Airport = airportWithoutMetar,
            RawTaf = "TAF",
            IssuedAt = DateTimeOffset.UtcNow
        });
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await new GetAirportSummariesFeature(Context).GetAirportSummariesAsync();

        var summary = Assert.Single(result);
        Assert.Equal("EHAM", summary.Icao);
        Assert.Null(summary.LatestForecast);
    }

    [Fact]
    public async Task GetAirportSummariesAsync_WithTafWithoutIssueTime_UsesMinimumDate()
    {
        var airport = new Airport { Icao = "EHAM", Name = "Schiphol" };
        Context.Airports.Add(airport);
        Context.Metars.Add(new Metar
        {
            Airport = airport,
            RawMetar = "METAR",
            IssuedAt = DateTimeOffset.UtcNow
        });
        Context.Tafs.Add(new Taf { Airport = airport, RawTaf = "TAF", IssuedAt = null });
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await new GetAirportSummariesFeature(Context).GetAirportSummariesAsync();

        Assert.Equal(DateTimeOffset.MinValue, Assert.Single(result).LatestForecast!.IssuedAt);
    }

    [Fact]
    public async Task GetAirportSummariesAsync_WithMoreThanOneHundredAirports_ReturnsAtMostOneHundred()
    {
        for (var index = 0; index < 101; index++)
        {
            var airport = new Airport { Icao = $"EH{index:00}", Name = $"Airport {index}" };
            Context.Airports.Add(airport);
            Context.Metars.Add(new Metar
            {
                Airport = airport,
                RawMetar = $"METAR {index}",
                IssuedAt = DateTimeOffset.UtcNow
            });
        }

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await new GetAirportSummariesFeature(Context).GetAirportSummariesAsync();

        Assert.Equal(100, result.Count);
    }
}
