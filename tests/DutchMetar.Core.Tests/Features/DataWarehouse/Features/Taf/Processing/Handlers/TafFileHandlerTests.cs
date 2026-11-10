using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;
using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TafEntity = DutchMetar.Core.Domain.Entities.Taf;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Taf.Processing.Handlers;

public class TafFileHandlerTests : TestsWithContext
{
    private readonly IKnmiApiClient _client = Substitute.For<IKnmiApiClient>();
    private readonly IRawTafFileParser _parser = Substitute.For<IRawTafFileParser>();
    private readonly TafFileHandler _feature;

    public TafFileHandlerTests()
    {
        _feature = new TafFileHandler(
            Context,
            Substitute.For<ILogger<TafFileHandler>>(),
            _client,
            _parser);
    }

    [Fact]
    public async Task HandleFileAsync_WithEmptyFileName_ReturnsWithoutDownloading()
    {
        await _feature.HandleFileAsync(
            new KnmiFileMeta { FileName = " ", CreatedOn = DateTimeOffset.UtcNow },
            CancellationToken.None);

        await _client.DidNotReceive().GetDatasetFileContentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        Assert.Empty(Context.KnmiTafFiles);
    }

    [Fact]
    public async Task HandleFileAsync_WhenFileAlreadyExists_DoesNotDownloadAgain()
    {
        var createdOn = DateTimeOffset.UtcNow;
        Context.KnmiTafFiles.Add(new KnmiTafFile
        {
            FileName = "existing.txt",
            FileContent = "existing content",
            FileCreatedAt = createdOn,
            FileLastModifiedAt = createdOn
        });
        await Context.SaveChangesAsync();

        await _feature.HandleFileAsync(
            new KnmiFileMeta { FileName = "existing.txt", CreatedOn = createdOn },
            CancellationToken.None);

        await _client.DidNotReceive().GetDatasetFileContentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        Assert.Single(Context.KnmiTafFiles);
    }

    [Fact]
    public async Task HandleFileAsync_WhenDownloadedContentIsEmpty_DoesNotSaveFile()
    {
        var fileMeta = new KnmiFileMeta { FileName = "empty.txt", CreatedOn = DateTimeOffset.UtcNow };
        _client.GetDatasetFileContentAsync("taf", fileMeta.FileName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(string.Empty));

        await _feature.HandleFileAsync(fileMeta, CancellationToken.None);

        Assert.Empty(Context.KnmiTafFiles);
        _parser.DidNotReceive().ParseRawTafToEntity(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleFileAsync_WhenParserThrows_SavesFileAsUnprocessed()
    {
        var fileMeta = new KnmiFileMeta { FileName = "invalid.txt", CreatedOn = DateTimeOffset.UtcNow };
        _client.GetDatasetFileContentAsync("taf", fileMeta.FileName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("invalid TAF"));
        _parser.ParseRawTafToEntity(Arg.Any<string>()).Returns(_ => throw new TafParsingException("invalid"));

        await _feature.HandleFileAsync(fileMeta, CancellationToken.None);

        var savedFile = Assert.Single(Context.KnmiTafFiles);
        Assert.Equal("invalid.txt", savedFile.FileName);
        Assert.Equal("invalid TAF", savedFile.FileContent);
        Assert.False(savedFile.IsFileProcessed);
        Assert.Empty(Context.Tafs);
    }

    [Fact]
    public async Task HandleFileAsync_WithValidTafAndExistingAirport_ReusesAirportAndSavesMetadata()
    {
        var airport = new Airport { Icao = "EHAM", Name = "Schiphol" };
        Context.Airports.Add(airport);
        await Context.SaveChangesAsync();

        var fileMeta = new KnmiFileMeta
        {
            FileName = "taf.txt",
            CreatedOn = new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero)
        };
        _client.GetDatasetFileContentAsync("taf", fileMeta.FileName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("valid TAF"));
        _parser.ParseRawTafToEntity("valid TAF").Returns(new TafEntity
        {
            Airport = new Airport { Icao = "eham" },
            RawTaf = "parsed TAF",
            IssuedAt = fileMeta.CreatedOn
        });

        await _feature.HandleFileAsync(fileMeta, CancellationToken.None);

        var savedFile = Assert.Single(Context.KnmiTafFiles);
        Assert.Equal(fileMeta.CreatedOn, savedFile.FileCreatedAt);
        Assert.Equal(fileMeta.CreatedOn, savedFile.FileLastModifiedAt);
        Assert.Equal("valid TAF", savedFile.FileContent);
        Assert.Equal("parsed TAF", savedFile.ExtractedRawTaf);
        Assert.True(savedFile.IsFileProcessed);

        var savedTaf = await Context.Tafs.Include(x => x.Airport).SingleAsync();
        Assert.Equal("KNMI", savedTaf.Source);
        Assert.Equal(fileMeta.FileName, savedTaf.SourceFileName);
        Assert.Equal("parsed TAF", savedTaf.RawTaf);
        Assert.Equal(airport.Id, savedTaf.AirportId);
        Assert.Equal("EHAM", savedTaf.Airport!.Icao);
        Assert.Equal(1, await Context.Airports.CountAsync());
    }

    [Fact]
    public async Task HandleFileAsync_WithValidTafAndUnknownAirport_SavesParsedAirport()
    {
        var fileMeta = new KnmiFileMeta { FileName = "new-airport.txt", CreatedOn = DateTimeOffset.UtcNow };
        _client.GetDatasetFileContentAsync("taf", fileMeta.FileName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("valid TAF"));
        _parser.ParseRawTafToEntity("valid TAF").Returns(new TafEntity
        {
            Airport = new Airport { Icao = "EHXX", Name = "New airport" },
            RawTaf = "parsed TAF"
        });

        await _feature.HandleFileAsync(fileMeta, CancellationToken.None);

        var savedTaf = await Context.Tafs.Include(x => x.Airport).SingleAsync();
        Assert.Equal("EHXX", savedTaf.Airport!.Icao);
        Assert.Equal("New airport", savedTaf.Airport.Name);
        Assert.Equal(1, await Context.Airports.CountAsync());
    }
}
