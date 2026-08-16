using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Notifications;
using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Taf.Notifications;

public class NewTafNotificationFeatureTests : TestsWithContext
{
    private readonly NewTafNotificationFeature _feature;
    private readonly IKnmiApiClient _apiClient;
    private readonly IRawTafFileParser _tafParser;

    private const string EhggTafPayload = """
                                          ZCZC
                                          FT150500 EHGG AAA
                                          TAF AMD EHGG 150739Z 1507/1612 33004KT 9999 BKN007
                                              TEMPO 1507/1512 7000 SHRA FEW090CB
                                              BECMG 1509/1512 02010KT CAVOK
                                              BECMG 1521/1524 34003KT=
                                          """;

    public NewTafNotificationFeatureTests()
    {
        _apiClient = Substitute.For<IKnmiApiClient>();
        var logger = Substitute.For<ILogger<NewTafNotificationFeature>>();
        _tafParser = Substitute.For<IRawTafFileParser>();
        _feature = new NewTafNotificationFeature(_apiClient, logger, Context, _tafParser);
    }

    [Fact]
    public void CanHandleMessage_TafDataSet_ReturnsTrue()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData
            {
                DataSetName = KnmiDatasetNames.Taf
            }
        };
        
        var canHandleMessage = _feature.CanHandleMessage(fileEvent);
        
        Assert.True(canHandleMessage);
    }
    
    [Fact]
    public void CanHandleMessage_OtherDataSet_ReturnsFalse()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData
            {
                DataSetName = KnmiDatasetNames.Metar
            }
        };
        
        var canHandleMessage = _feature.CanHandleMessage(fileEvent);
        
        Assert.False(canHandleMessage);
    }

    [Fact]
    public async Task HandleNotificationAsync_ValidTafNewAirport_SavesTafAndAirport()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData
            {
                DataSetName = KnmiDatasetNames.Taf,
                DataSetVersion = "v1",
                FileName = "taf_mock_test_ehgg.txt",
                Url = "https://localhost/no/where/taf_mock_test_ehgg.txt"
            }
        };
        _apiClient
            .GetDatasetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(EhggTafPayload);
        _tafParser.ParseRawTafToEntity(Arg.Any<string>()).Returns(new Domain.Entities.Taf
        {
            RawTaf = "TAF AMD EHGG 150739Z 1507/1612 33004KT 9999 BKN007\n    TEMPO 1507/1512 7000 SHRA FEW090CB\n    BECMG 1509/1512 02010KT CAVOK\n    BECMG 1521/1524 34003KT=",
            IssuedAt = DateTimeOffset.Now,
            Airport = new Airport
            {
                Icao = "EHGG"
            }
        });

        await _feature.HandleNotificationAsync(fileEvent, CancellationToken.None);

        var allTafs = await Context.Tafs
            .Include(x => x.Airport)
            .ToArrayAsync();

        var taf = allTafs.First();
        Assert.NotNull(taf.Airport);
    }

    [Fact]
    public async Task HandleNotificationAsync_WhenTafIsValidAndAirportAlreadyExists_UsesExistingAirportEntity()
    {
        var existingAirport = new Airport { Icao = "EHGG" };
        Context.Airports.Add(existingAirport);
        await Context.SaveChangesAsync();

        var fileEvent = new FileEvent
        {
            Data = new FileData
            {
                DataSetName = KnmiDatasetNames.Taf,
                DataSetVersion = "v1",
                FileName = "taf_mock_test_ehgg.txt",
                Url = "https://localhost/no/where/taf_mock_test_ehgg.txt"
            }
        };

        _apiClient
            .GetDatasetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(EhggTafPayload);
        _tafParser.ParseRawTafToEntity(Arg.Any<string>()).Returns(new Domain.Entities.Taf
        {
            RawTaf = "TAF AMD EHGG 150739Z 1507/1612 33004KT 9999 BKN007",
            IssuedAt = DateTimeOffset.Now,
            Airport = new Airport { Icao = "EHGG" }
        });

        await _feature.HandleNotificationAsync(fileEvent, CancellationToken.None);

        var savedTaf = await Context.Tafs
            .Include(x => x.Airport)
            .SingleAsync();

        Assert.Equal(existingAirport.Id, savedTaf.Airport!.Id);
        Assert.Equal(existingAirport.Icao, savedTaf.Airport.Icao);
    }

    [Fact]
    public async Task HandleNotificationAsync_WhenDataSetNameIsEmpty_DoesNothing()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData
            {
                DataSetName = string.Empty
            }
        };

        await _feature.HandleNotificationAsync(fileEvent, CancellationToken.None);

        Assert.Empty(await Context.Tafs.ToListAsync());
        Assert.DoesNotContain(
            _apiClient.ReceivedCalls(),
            call => call.GetMethodInfo().Name == nameof(IKnmiApiClient.GetDatasetFileContentAsync));
    }

    [Fact]
    public async Task HandleNotificationAsync_WhenFileContentIsEmpty_DoesNothing()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData
            {
                DataSetName = KnmiDatasetNames.Taf,
                FileName = "empty.txt"
            }
        };

        _apiClient
            .GetDatasetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        await _feature.HandleNotificationAsync(fileEvent, CancellationToken.None);

        Assert.Empty(await Context.Tafs.ToListAsync());
        Assert.DoesNotContain(
            _tafParser.ReceivedCalls(),
            call => call.GetMethodInfo().Name == nameof(IRawTafFileParser.ParseRawTafToEntity));
    }

    [Fact]
    public async Task HandleNotificationAsync_WhenTafParsingFails_DoesNothing()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData
            {
                DataSetName = KnmiDatasetNames.Taf,
                FileName = "invalid.txt"
            }
        };

        _apiClient
            .GetDatasetFileContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(EhggTafPayload);
        _tafParser
            .ParseRawTafToEntity(Arg.Any<string>())
            .Returns(x => throw new TafParsingException("invalid"));

        await _feature.HandleNotificationAsync(fileEvent, CancellationToken.None);

        Assert.Empty(await Context.Tafs.ToListAsync());
    }
}