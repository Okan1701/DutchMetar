using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Shared;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiNotifications;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Shared;

public class NewKnmiFileHandlerTests : TestsWithContext
{
    [Fact]
    public async Task HandleFileAsync_WhenFileExists_LogsAndReturns()
    {
        var fileMeta = new KnmiFileMeta { FileName = "exists.xml", CreatedOn = DateTimeOffset.UtcNow };
        Context.KnmiMetarFiles.Add(new KnmiMetarFile { FileName = fileMeta.FileName, FileContent = "x", FileCreatedAt = fileMeta.CreatedOn, FileLastModifiedAt = fileMeta.CreatedOn });
        await Context.SaveChangesAsync();

        var logger = Substitute.For<ILogger<NewKnmiFileHandler>>();
        var client = Substitute.For<IKnmiMetarApiClient>();
        var parser = Substitute.For<IMetarXmlParser>();

        var sut = new NewKnmiFileHandler(Context, logger, client, parser);

        await sut.HandleFileAsync(fileMeta, CancellationToken.None);

        Assert.Equal(1, Context.KnmiMetarFiles.Count());
    }

    [Fact]
    public async Task HandleFileAsync_WhenContentEmpty_DoesNotSaveFile()
    {
        var fileMeta = new KnmiFileMeta { FileName = "empty.xml", CreatedOn = DateTimeOffset.UtcNow };
        var logger = Substitute.For<ILogger<NewKnmiFileHandler>>();
        var client = Substitute.For<IKnmiMetarApiClient>();
        client.GetKnmiMetarFileContentAsync(fileMeta.FileName, Arg.Any<CancellationToken>()).Returns(Task.FromResult(string.Empty));
        var parser = Substitute.For<IMetarXmlParser>();

        var sut = new NewKnmiFileHandler(Context, logger, client, parser);

        await sut.HandleFileAsync(fileMeta, CancellationToken.None);

        var saved = Context.KnmiMetarFiles.SingleOrDefault(x => x.FileName == fileMeta.FileName);
        Assert.Null(saved);
    }

    [Fact]
    public async Task HandleFileAsync_WhenParserThrows_LogsErrorAndSavesFile()
    {
        var fileMeta = new KnmiFileMeta { FileName = "bad.xml", CreatedOn = DateTimeOffset.UtcNow };
        var logger = Substitute.For<ILogger<NewKnmiFileHandler>>();
        var client = Substitute.For<IKnmiMetarApiClient>();
        client.GetKnmiMetarFileContentAsync(fileMeta.FileName, Arg.Any<CancellationToken>()).Returns(Task.FromResult("<xml></xml>"));
        var parser = Substitute.For<IMetarXmlParser>();
        parser.When(x => x.Map(Arg.Any<string>())).Do(_ => throw new MetarXmlParsingException("fail"));

        var sut = new NewKnmiFileHandler(Context, logger, client, parser);

        await sut.HandleFileAsync(fileMeta, CancellationToken.None);

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
        var logger = Substitute.For<ILogger<NewKnmiFileHandler>>();
        var client = Substitute.For<IKnmiMetarApiClient>();
        client.GetKnmiMetarFileContentAsync(fileMeta.FileName, Arg.Any<CancellationToken>()).Returns(Task.FromResult("<xml></xml>"));
        var parser = Substitute.For<IMetarXmlParser>();
        var metar = new DutchMetar.Core.Domain.Entities.Metar { RawMetar = "raw", IssuedAt = DateTimeOffset.UtcNow, Airport = new Airport { Icao = "EHAM" } };
        parser.Map(Arg.Any<string>()).Returns(metar);

        var sut = new NewKnmiFileHandler(Context, logger, client, parser);

        await sut.HandleFileAsync(fileMeta, CancellationToken.None);

        var savedFile = Context.KnmiMetarFiles.Single(x => x.FileName == fileMeta.FileName);
        Assert.True(savedFile.IsFileProcessed);
        Assert.Equal("raw", savedFile.ExtractedRawMetar);
        Assert.Equal(1, Context.Metars.Count());
        Assert.Equal(airport.Icao, Context.Metars.Include(m => m.Airport!).Single().Airport!.Icao);
    }
}
