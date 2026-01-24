using DutchMetar.Core.Features.DataWarehouse.Shared;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Infrastructure.Accessors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.SyncKnmiMetarFileList.Services;

public class MetarProcessorTests : TestsWithContext
{
    private readonly MetarProcessor _metarProcessor;

    public MetarProcessorTests()
    {
        var logger = Substitute.For<ILogger<MetarProcessor>>();
        _metarProcessor = new MetarProcessor(new MetarMapper(), logger, Context, new SimpleCorrelationIdAccessor());
    }

    [Fact]
    public async Task ProcessRawMetarAsync_ValidEhrdMetar_MetarDecodedAndSaved()
    {
        const string rawMetar = "METAR EHRD 171355Z AUTO 11003KT 9999 SCT015/// 11/08 Q1018 NOSIG=";
        const string airportName = "ROTTERDAM/THE HAGUE AIRPORT";
        
        // Act
        await _metarProcessor.ProcessRawMetarAsync(rawMetar, airportName, DateTimeOffset.Now, CancellationToken.None);
        
        // Assert
        var airportWithMetars = await Context.Airports
            .Include(x => x.MetarReports)
            .FirstOrDefaultAsync(x => x.Icao == "EHRD");
        
        Assert.NotNull(airportWithMetars);
        Assert.Single(airportWithMetars.MetarReports);
        Assert.Equal("EHRD", airportWithMetars.Icao);
        Assert.Equal(airportName, airportWithMetars.Name);
    }
    
    [Fact]
    public async Task ProcessRawMetarAsync_ValidMetarExistingAIrportEntity_NoDuplicateAirportSaved()
    {
        // Arrange
        const string rawMetar = "METAR EHRD 171355Z AUTO 11003KT 9999 SCT015/// 11/08 Q1018 NOSIG=";
        const string airportName = "ROTTERDAM/THE HAGUE AIRPORT";
        Context.Airports.Add(new()
        {
            Icao = "EHRD"
        });
        await Context.SaveChangesAsync();
        
        // Act
        await _metarProcessor.ProcessRawMetarAsync(rawMetar, airportName, DateTimeOffset.Now, CancellationToken.None);
        
        // Assert
        Assert.Single(Context.Airports);
    }
    
    [Fact]
    public async Task ProcessRawMetarAsync_DuplicateMetar_DuplicateNotSaved()
    {
        // Arrange
        const string rawMetar = "METAR EHRD 171355Z AUTO 11003KT 9999 SCT015/// 11/08 Q1018 NOSIG=";
        
        // Act
        await _metarProcessor.ProcessRawMetarAsync(rawMetar, null, DateTimeOffset.Now, CancellationToken.None);
        await _metarProcessor.ProcessRawMetarAsync(rawMetar, null, DateTimeOffset.Now, CancellationToken.None);
        
        // Assert
        Assert.Single(Context.Airports);
        Assert.Single(Context.Metars);
    }
    
    [Fact]
    public async Task ProcessRawMetarAsync_CorrectedMetar_OldMetarReplaced()
    {
        // Arrange
        const string rawMetar = "METAR EHRD 171355Z AUTO 11003KT 9999 SCT015/// 11/08 Q1018 NOSIG=";
        const string rawMetarCor = "METAR EHRD COR 171355Z AUTO 11003KT 9999 SCT015/// 20/08 Q1018 NOSIG=";
        
        // Act
        await _metarProcessor.ProcessRawMetarAsync(rawMetar, null, DateTimeOffset.Now, CancellationToken.None);
        await _metarProcessor.ProcessRawMetarAsync(rawMetarCor, null, DateTimeOffset.Now, CancellationToken.None);
        
        // Assert
        Assert.Single(Context.Airports);
        Assert.Single(Context.Metars);
        var metar = Context.Metars.First();
        Assert.Equal(20, metar.TemperatureCelsius);
        Assert.True(metar.IsCorrected);
    }

    [Fact]
    public async Task ProcessRawMetarAsync_InvalidMetar_MetarParseExceptionThrown()
    {
        await Assert.ThrowsAsync<MetarParseException>(
            async () => await _metarProcessor.ProcessRawMetarAsync(Guid.NewGuid().ToString(), null, DateTimeOffset.Now, CancellationToken.None));
        
        Assert.Empty(Context.Airports);
        Assert.Empty(Context.Metars);
    }
    
    [Fact]
    public async Task ProcessRawMetarAsync_AirportNotDecoded_MetarParseExceptionThrown()
    {
        var metar = "METAR 171355Z 11003KT 9999 SCT015/// 11/08 Q1018 NOSIG=";
        await Assert.ThrowsAsync<MetarParseException>(
            async () => await _metarProcessor.ProcessRawMetarAsync(metar, null, DateTimeOffset.Now, CancellationToken.None));
        
        Assert.Empty(Context.Airports);
        Assert.Empty(Context.Metars);
    }
}