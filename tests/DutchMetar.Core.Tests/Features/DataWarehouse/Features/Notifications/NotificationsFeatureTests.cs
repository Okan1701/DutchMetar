using DutchMetar.Core.Features.DataWarehouse.Features.Notifications;
using DutchMetar.Core.Features.DataWarehouse.Shared;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Notifications;

public class NotificationsFeatureTests
{
    [Fact]
    public async Task HandleFileAsync_WithInvalidFile_DoesNotCallHandler()
    {
        var handler = Substitute.For<INewKnmiFileHandler>();
        var sut = new NotificationsFeature(handler);

        var invalid = new KnmiFileMeta { FileName = string.Empty, CreatedOn = DateTimeOffset.MinValue };

        await sut.HandleFileAsync(invalid, CancellationToken.None);

        await handler.DidNotReceiveWithAnyArgs().HandleFileAsync(default!, default);
    }

    [Fact]
    public async Task HandleFileAsync_WithValidFile_CallsHandler()
    {
        var handler = Substitute.For<INewKnmiFileHandler>();
        var sut = new NotificationsFeature(handler);

        var valid = new KnmiFileMeta { FileName = "file1", CreatedOn = DateTimeOffset.UtcNow };

        await sut.HandleFileAsync(valid, CancellationToken.None);

        await handler.Received(1).HandleFileAsync(valid, CancellationToken.None);
    }
}
