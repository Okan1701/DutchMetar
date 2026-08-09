using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Notifications;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Metar.Notifications;

public class NewMetarKnmiNotificationFeatureTests
{
    private readonly IMetarFileHandler _handler;
    private readonly NewMetarKnmiNotificationFeature _feature;

    public NewMetarKnmiNotificationFeatureTests()
    {
        _handler = Substitute.For<IMetarFileHandler>();
        _feature = new NewMetarKnmiNotificationFeature(_handler);
    }

    [Fact]
    public void CanHandleMessage_WithMetarDataset_ReturnsTrue()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData { DataSetName = KnmiDatasetNames.Metar, FileName = "file.xml" },
            Time = DateTimeOffset.UtcNow.ToString("O")
        };

        Assert.True(_feature.CanHandleMessage(fileEvent));
    }

    [Fact]
    public void CanHandleMessage_WithNonMetarDataset_ReturnsFalse()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData { DataSetName = "taf", FileName = "file.xml" },
            Time = DateTimeOffset.UtcNow.ToString("O")
        };

        Assert.False(_feature.CanHandleMessage(fileEvent));
    }

    [Fact]
    public void CanHandleMessage_WithNullData_ReturnsFalse()
    {
        var fileEvent = new FileEvent { Data = null, Time = DateTimeOffset.UtcNow.ToString("O") };

        Assert.False(_feature.CanHandleMessage(fileEvent));
    }

    [Fact]
    public async Task HandleNotificationAsync_WithInvalidFileEvent_DoesNotCallHandler()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData { DataSetName = KnmiDatasetNames.Metar, FileName = string.Empty },
            Time = null
        };

        await _feature.HandleNotificationAsync(fileEvent, CancellationToken.None);

        await _handler.DidNotReceiveWithAnyArgs().HandleFileAsync(default!, default);
    }

    [Fact]
    public async Task HandleNotificationAsync_WithMissingFileName_DoesNotCallHandler()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData { DataSetName = KnmiDatasetNames.Metar, FileName = string.Empty },
            Time = DateTimeOffset.UtcNow.ToString("O")
        };

        await _feature.HandleNotificationAsync(fileEvent, CancellationToken.None);

        await _handler.DidNotReceiveWithAnyArgs().HandleFileAsync(default!, default);
    }

    [Fact]
    public async Task HandleNotificationAsync_WithMissingTime_DoesNotCallHandler()
    {
        var fileEvent = new FileEvent
        {
            Data = new FileData { DataSetName = KnmiDatasetNames.Metar, FileName = "file.xml" },
            Time = null
        };

        await _feature.HandleNotificationAsync(fileEvent, CancellationToken.None);

        await _handler.DidNotReceiveWithAnyArgs().HandleFileAsync(default!, default);
    }

    [Fact]
    public async Task HandleNotificationAsync_WithValidFileEvent_CallsHandler()
    {
        var now = DateTimeOffset.UtcNow;
        var fileEvent = new FileEvent
        {
            Data = new FileData { DataSetName = KnmiDatasetNames.Metar, FileName = "file.xml" },
            Time = now.ToString("O")
        };

        await _feature.HandleNotificationAsync(fileEvent, CancellationToken.None);

        await _handler.Received(1).HandleFileAsync(Arg.Is<KnmiFileMeta>(x => x.FileName == "file.xml"), CancellationToken.None);
    }

    [Fact]
    public async Task HandleNotificationAsync_WithCancellationToken_PassesTokenToHandler()
    {
        var cts = new CancellationTokenSource();
        var fileEvent = new FileEvent
        {
            Data = new FileData { DataSetName = KnmiDatasetNames.Metar, FileName = "file.xml" },
            Time = DateTimeOffset.UtcNow.ToString("O")
        };

        await _feature.HandleNotificationAsync(fileEvent, cts.Token);

        await _handler.Received(1).HandleFileAsync(Arg.Any<KnmiFileMeta>(), cts.Token);
    }
}
