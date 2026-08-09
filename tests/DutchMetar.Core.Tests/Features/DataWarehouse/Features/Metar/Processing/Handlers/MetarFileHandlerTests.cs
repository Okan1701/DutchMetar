using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Parsers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Metar.Processing.Handlers;

public class MetarFileHandlerTests : TestsWithContext
{
    private readonly IKnmiApiClient _client;
    private readonly IMetarXmlParser _parser;
    private readonly MetarFileHandler _feature;

    public MetarFileHandlerTests()
    {
        var logger = Substitute.For<ILogger<MetarFileHandler>>();
        _client = Substitute.For<IKnmiApiClient>();
        _parser = Substitute.For<IMetarXmlParser>();
        _feature = new MetarFileHandler(Context, logger, _client, _parser);
    }

    [Fact]
    public async Task HandleFileAsync_WhenFileExists_LogsAndReturns()
    {
        var fileMeta = new KnmiFileMeta { FileName = "exists.xml", CreatedOn = DateTimeOffset.UtcNow };
        Context.KnmiMetarFiles.Add(new KnmiMetarFile { FileName = fileMeta.FileName, FileContent = "x", FileCreatedAt = fileMeta.CreatedOn, FileLastModifiedAt = fileMeta.CreatedOn });
        await Context.SaveChangesAsync();

        await _feature.HandleFileAsync(fileMeta, CancellationToken.None);

        Assert.Equal(1, Context.KnmiMetarFiles.Count());
    }

    [Fact]
    public async Task HandleFileAsync_WhenContentEmpty_DoesNotSaveFile()
    {
        var fileMeta = new KnmiFileMeta { FileName = "empty.xml", CreatedOn = DateTimeOffset.UtcNow };
        _client.GetDatasetFileContentAsync("metar", fileMeta.FileName, Arg.Any<CancellationToken>()).Returns(Task.FromResult(string.Empty));

        await _feature.HandleFileAsync(fileMeta, CancellationToken.None);

        var saved = Context.KnmiMetarFiles.SingleOrDefault(x => x.FileName == fileMeta.FileName);
        Assert.Null(saved);
    }

    [Fact]
    public async Task HandleFileAsync_WhenParserThrows_LogsErrorAndSavesFile()
    {
        var fileMeta = new KnmiFileMeta { FileName = "bad.xml", CreatedOn = DateTimeOffset.UtcNow };
        _client.GetDatasetFileContentAsync("metar", fileMeta.FileName, Arg.Any<CancellationToken>()).Returns(Task.FromResult("<xml></xml>"));
        _parser.When(x => x.Map(Arg.Any<string>())).Do(_ => throw new MetarXmlParsingException("fail"));

        await _feature.HandleFileAsync(fileMeta, CancellationToken.None);

        var saved = Context.KnmiMetarFiles.SingleOrDefault(x => x.FileName == fileMeta.FileName);
        Assert.NotNull(saved);
        Assert.False(saved.IsFileProcessed);
    }

    [Fact]
    public async Task HandleFileAsync_WhenValidContentAndExistingAirport_ReusesAirportAndAddsMetar()
    {
        var airport = new Airport { Icao = "EHAM", Name = "Schiphol" };
        Context.Airports.Add(airport);
        await Context.SaveChangesAsync();

        var fileMeta = new KnmiFileMeta { FileName = "good.xml", CreatedOn = DateTimeOffset.UtcNow };
        _client.GetDatasetFileContentAsync("metar", fileMeta.FileName, Arg.Any<CancellationToken>()).Returns(Task.FromResult("<xml></xml>"));
        var metar = new DutchMetar.Core.Domain.Entities.Metar { RawMetar = "raw", IssuedAt = DateTimeOffset.UtcNow, Airport = new Airport { Icao = "EHAM" } };
        _parser.Map(Arg.Any<string>()).Returns(metar);

        await _feature.HandleFileAsync(fileMeta, CancellationToken.None);

        var savedFile = Context.KnmiMetarFiles.Single(x => x.FileName == fileMeta.FileName);
        Assert.True(savedFile.IsFileProcessed);
        Assert.Equal("raw", savedFile.ExtractedRawMetar);
        Assert.Equal(1, Context.Metars.Count());
        Assert.Equal(airport.Icao, Context.Metars.Include(m => m.Airport!).Single().Airport!.Icao);
    }
}
