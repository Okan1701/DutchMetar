using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.LoadDutchMetars;
using DutchMetar.Core.Features.LoadDutchMetars.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.LoadDutchMetars;

public class LoadDutchMetarsFeatureTests : IDisposable
{
    private readonly LoadDutchMetarsFeature _feature;
    private readonly DutchMetarContext _context;
    private readonly IKnmiMetarRepository _knmiMetarRepository;

    public LoadDutchMetarsFeatureTests()
    {
        var builder = new DbContextOptionsBuilder<DutchMetarContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _knmiMetarRepository = Substitute.For<IKnmiMetarRepository>();
        _knmiMetarRepository.GetKnmiRawMetarsAsync().ReturnsForAnyArgs([
            "METAR EHRD 271325Z AUTO 12002KT 6000 NCD 17/09 Q1020=",
            "METAR EHAM 271325Z AUTO 09015KT 9999 NCD 17/09 Q1020=",
            "METAR EHLE 271325Z AUTO 07520KT 9999 SCT015 17/09 Q1020=",
            "METAR EHFS 271325Z AUTO 03002KT 5000 NCD 17/09 Q1020=",
            "METAR EHKD 271355Z AUTO 09007KT 060V120 9999 SCT015 SCT020 BKN025 15/11 Q1021 BLU NOSIG="
        ]);
        _context = new DutchMetarContext(builder.Options);
        _feature = new(_knmiMetarRepository, _context, Substitute.For<ILogger<LoadDutchMetarsFeature>>(), new MetarMapper());
        
        #region Test data
        _context.Airports.AddRange([
            new Airport
            {
                Icao = "EHAM",
                CreatedAt = DateTime.Today,
                LastUpdatedAt = DateTime.Today,
                MetarReports = []
            },
            new Airport
            {
                Icao = "EHLE",
                CreatedAt = DateTime.Today,
                LastUpdatedAt = DateTime.Today,
                MetarReports = [new Metar
                {
                    CreatedAt = DateTime.Today,
                    LastUpdatedAt = DateTime.Today,
                    IssuedAt = DateTime.Today.AddMonths(-1),
                    RawMetar = ""
                }]
            },
            new Airport
            {
                Icao = "EHFS",
                CreatedAt = DateTime.Today,
                LastUpdatedAt = DateTime.Today,
                MetarReports = [new Metar
                {
                    CreatedAt = DateTime.Today,
                    LastUpdatedAt = DateTime.Today,
                    IssuedAt = DateTime.Today.AddMonths(1),
                    RawMetar = ""
                }]
            }
        ]);
        _context.SaveChanges();

        #endregion
    }

    public void Dispose()
    {
        _context.Airports.RemoveRange(_context.Airports);
        _context.Metars.RemoveRange(_context.Metars);
        _context.SaveChanges();
        _context.Dispose();
    }

    [Fact]
    public async Task RetrieveDutchMetarsAsync_ValidMetars_AllValidMetarsSaved()
    {
        await _feature.LoadAsync();
        var airports = _context.Airports.Include(x => x.MetarReports).ToArray();
        
        Assert.NotNull(airports);
        Assert.Equal(5, airports.Length);
        Assert.True(airports.All(x => x.MetarReports.Count > 0));
    }

    [Fact]
    public async Task RetrieveDutchMetarsAsync_CorrectionToExistingMetar_ExistingMetarUpdated()
    {
        var latestMetarIssuedAt = DateTime.UtcNow;
        _context.Metars.Add(new Metar
        {
            RawMetar = $"METAR EHLE {latestMetarIssuedAt:dd}1325Z AUTO 12002KT 6000 NCD 17/09 Q1020=",
            Airport = await _context.Airports.FirstAsync(x => x.Icao == "EHLE"),
            IssuedAt = latestMetarIssuedAt,
        });
        await _context.SaveChangesAsync();
        _knmiMetarRepository.GetKnmiRawMetarsAsync().ReturnsForAnyArgs([
            $"METAR EHLE {latestMetarIssuedAt:dd}1325Z AUTO COR 24009KT 9999 NCD 21/08 Q1025=",
        ]);
        
        await _feature.LoadAsync();
        var latestEhleMetar = await _context.Metars
            .Where(x => x.Airport!.Icao == "EHLE")
            .OrderByDescending(x => x.IssuedAt)
            .FirstAsync();
        
        Assert.True(latestEhleMetar.IsCorrected);
        Assert.Equal(9999, latestEhleMetar.VisibilityMeters);
        Assert.Equal(21, latestEhleMetar.TemperatureCelsius);
        Assert.Equal(8, latestEhleMetar.DewpointCelsius);
        Assert.Equal(1025, latestEhleMetar.AltimeterValue);
    }
    
    [Fact]
    public async Task RetrieveDutchMetarsAsync_NoMetars_NothingNewSaved()
    {
        _knmiMetarRepository.GetKnmiRawMetarsAsync().ReturnsForAnyArgs([]);
        var existingAirportsCount = await _context.Airports.CountAsync();
        var existingMetarsCount = await _context.Metars.CountAsync();
        
        await _feature.LoadAsync();
        var airports = _context.Airports.Include(x => x.MetarReports).ToArray();
        
        Assert.NotNull(airports);
        Assert.Equal(existingAirportsCount, airports.Length);
        Assert.True(airports.SelectMany(x => x.MetarReports).Count() == existingMetarsCount);
    }
}