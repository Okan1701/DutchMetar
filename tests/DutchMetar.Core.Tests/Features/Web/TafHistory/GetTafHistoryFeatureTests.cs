using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Exceptions;
using DutchMetar.Core.Features.Web.TafHistory;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.Web.TafHistory;

public class GetTafHistoryFeatureTests : TestsWithContext
{
    private readonly Airport _testAirport;
    private readonly GetTafHistoryFeature _feature;

    public GetTafHistoryFeatureTests() : base()
    {
        _testAirport = new Airport
        {
            Icao = "EHXX",
            Name = "TEST"
        };

        Context.Airports.Add(_testAirport);
        Context.SaveChanges();

        _feature = new GetTafHistoryFeature(Substitute.For<ILogger<GetTafHistoryFeature>>(), Context);
    }

    [Fact]
    public async Task GetHistory_ExistingIcaoWithTafs_ReturnsPaginatedReportsIncludingUnissuedTaf()
    {
        Context.Tafs.AddRange(
            new Taf
            {
                AirportId = _testAirport.Id,
                Airport = _testAirport,
                RawTaf = "OLDER",
                IssuedAt = DateTimeOffset.UtcNow.AddHours(-1)
            },
            new Taf
            {
                AirportId = _testAirport.Id,
                Airport = _testAirport,
                RawTaf = "NEWER",
                IssuedAt = DateTimeOffset.UtcNow
            },
            new Taf
            {
                AirportId = _testAirport.Id,
                Airport = _testAirport,
                RawTaf = "NO ISSUE TIME",
                IssuedAt = null
            });
        await Context.SaveChangesAsync();

        var result = await _feature.GetHistoryAsync(new GetTafHistoryRequest
        {
            Icao = _testAirport.Icao,
            Page = 0,
            PageSize = 2
        });

        Assert.Equal(_testAirport.Icao, result.Icao);
        Assert.Equal(_testAirport.Name, result.AirportName);
        Assert.Equal(0, result.CurrentPage);
        Assert.Equal(2, result.MaxPages);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(new[] { "NEWER", "OLDER" }, result.TafReports.Select(x => x.RawTaf));
        Assert.Null((await _feature.GetHistoryAsync(new GetTafHistoryRequest
        {
            Icao = _testAirport.Icao,
            Page = 1,
            PageSize = 2
        })).TafReports.Single().IssuedAt);
    }

    [Fact]
    public async Task GetHistory_DateRange_IncludesBoundaryDatesAndExcludesNullIssueTime()
    {
        var date = new DateTimeOffset(2026, 10, 4, 0, 0, 0, TimeSpan.Zero);
        Context.Tafs.AddRange(
            CreateTaf("BEFORE", date.AddTicks(-1)),
            CreateTaf("START", date),
            CreateTaf("END", date.AddDays(1).AddTicks(-1)),
            CreateTaf("AFTER", date.AddDays(1)),
            CreateTaf("NULL", null));
        await Context.SaveChangesAsync();

        var result = await _feature.GetHistoryAsync(new GetTafHistoryRequest
        {
            Icao = _testAirport.Icao,
            Page = 0,
            StartDate = date,
            EndDate = date
        });

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.TafReports.Count);
        Assert.DoesNotContain(result.TafReports, x => x.RawTaf == "NULL");
    }

    [Fact]
    public async Task GetHistory_NonExistingIcao_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _feature.GetHistoryAsync(new GetTafHistoryRequest
        {
            Icao = "XXXX",
            Page = 0
        }));
    }

    private Taf CreateTaf(string rawTaf, DateTimeOffset? issuedAt)
    {
        return new Taf
        {
            AirportId = _testAirport.Id,
            Airport = _testAirport,
            RawTaf = rawTaf,
            IssuedAt = issuedAt
        };
    }
}
