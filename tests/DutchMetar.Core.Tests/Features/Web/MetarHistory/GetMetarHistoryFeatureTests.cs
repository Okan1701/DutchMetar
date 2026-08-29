using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Exceptions;
using DutchMetar.Core.Features.Web.MetarHistory;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.Web.MetarHistory;

public class GetMetarHistoryFeatureTests : TestsWithContext
{
    private readonly Airport _testAirport;
    private readonly GetMetarHistoryFeature _feature;
    
    public GetMetarHistoryFeatureTests() : base()
    {
        _testAirport = new Airport
        {
            Icao = "EHXX",
            Name = "TEST"
        };
        
        Context.Airports.Add(_testAirport);
        Context.SaveChanges();

        _feature = new GetMetarHistoryFeature(Substitute.For<ILogger<GetMetarHistoryFeature>>(), Context);
    }

    [Fact]
    public async Task GetHistory_ExistingIcaoWithMetars_ReturnsResultWithMetar()
    {
        Context.Metars.Add(new Metar
        {
            AirportId = _testAirport.Id,
            Airport = _testAirport,
            RawMetar = "XYZ",
        });
        await Context.SaveChangesAsync();

        var result = await _feature.GetHistoryAsync(new GetMetarHistoryRequest
        {
            Icao = _testAirport.Icao,
            Page = 1
        });

        Assert.NotNull(result);
        Assert.Equal(_testAirport.Icao, result.Icao);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(1, result.MaxPages);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(_testAirport.Name, result.AirportName);
        Assert.Single(result.MetarReports);
        Assert.Equal("XYZ", result.MetarReports.First().RawMetar);
    }
    
    [Fact]
    public async Task GetHistory_ExistingIcaoWithMultiplePages_ReturnsPaginatedResult()
    {
        for (var i = 0; i < 101; i++)
        {
            Context.Metars.Add(new Metar
            {
                AirportId = _testAirport.Id,
                Airport = _testAirport,
                RawMetar = i.ToString(),
            });
        }
        await Context.SaveChangesAsync();

        var result = await _feature.GetHistoryAsync(new GetMetarHistoryRequest
        {
            Icao = _testAirport.Icao,
            PageSize = 25,
            Page = 1
        });

        Assert.NotNull(result);
        Assert.Equal(_testAirport.Icao, result.Icao);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(5, result.MaxPages);
        Assert.Equal(101, result.TotalItems);
        Assert.Equal(25, result.MetarReports.Count);
    }

    [Fact]
    public async Task GetHistory_ExistingIcaoWithSecondPage_ReturnsCorrectSubset()
    {
        for (var i = 0; i < 5; i++)
        {
            Context.Metars.Add(new Metar
            {
                AirportId = _testAirport.Id,
                Airport = _testAirport,
                IssuedAt = DateTimeOffset.UtcNow.AddMinutes(i),
                RawMetar = $"METAR-{i}"
            });
        }
        await Context.SaveChangesAsync();

        var result = await _feature.GetHistoryAsync(new GetMetarHistoryRequest
        {
            Icao = _testAirport.Icao,
            PageSize = 2,
            Page = 2
        });

        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(3, result.MaxPages);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.MetarReports.Count);
        Assert.Equal("METAR-2", result.MetarReports.First().RawMetar);
        Assert.Equal("METAR-1", result.MetarReports.Last().RawMetar);
    }

    [Fact]
    public async Task GetHistory_ExistingIcaoWithLastPartialPage_ReturnsRemainingItems()
    {
        for (var i = 0; i < 5; i++)
        {
            Context.Metars.Add(new Metar
            {
                AirportId = _testAirport.Id,
                Airport = _testAirport,
                IssuedAt = DateTimeOffset.UtcNow.AddMinutes(i),
                RawMetar = $"REPORT-{i}"
            });
        }
        await Context.SaveChangesAsync();

        var result = await _feature.GetHistoryAsync(new GetMetarHistoryRequest
        {
            Icao = _testAirport.Icao,
            PageSize = 2,
            Page = 3
        });

        Assert.Equal(3, result.CurrentPage);
        Assert.Equal(3, result.MaxPages);
        Assert.Equal(5, result.TotalItems);
        Assert.Single(result.MetarReports);
        Assert.Equal("REPORT-0", result.MetarReports.First().RawMetar);
    }
    
    [Fact]
    public async Task GetHistory_NonExistingIcao_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(async () => await _feature.GetHistoryAsync(new GetMetarHistoryRequest
        {
            Icao = "XXXX",
            Page = 1
        }));
    }
    
    [Fact]
    public async Task GetHistory_ExistingIcaoWithNoMetars_ReturnsEmptyResult()
    {
        var result = await _feature.GetHistoryAsync(new GetMetarHistoryRequest
        {
            Icao = _testAirport.Icao,
            Page = 1
        });

        Assert.NotNull(result);
        Assert.Empty(result.MetarReports);
    }
}