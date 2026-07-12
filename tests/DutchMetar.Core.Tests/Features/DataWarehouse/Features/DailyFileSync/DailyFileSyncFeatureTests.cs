using DutchMetar.Core.Features.DataWarehouse.Features.DailyFileSync;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform.Exceptions;
using DutchMetar.Core.Infrastructure.Accessors;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.DailyFileSync;

public class DailyFileSyncFeatureTests
{
    [Fact]
    public async Task SyncKnmiMetarFiles_WhenRepositoryReturnsFiles_CallsHandlerForEach()
    {
        var logger = Substitute.For<ILogger<DailyFileSyncFeature>>();
        var repo = Substitute.For<IKnmiRepository>();
        var correlation = Substitute.For<ICorrelationIdAccessor>();
        correlation.CorrelationId.Returns(Guid.NewGuid());
        var handler = Substitute.For<INewKnmiFileHandler>();

        repo.GetKnmiMetarFiles(Arg.Any<DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform.Contracts.KnmiFilesParameters>(), Arg.Any<CancellationToken>(), Arg.Any<Guid>())
            .Returns(Task.FromResult<ICollection<KnmiFileMeta>>(new List<KnmiFileMeta>{ new KnmiFileMeta{ FileName = "file1", CreatedOn = DateTimeOffset.UtcNow } }));

        var sut = new DailyFileSyncFeature(logger, repo, correlation, handler);

        await sut.SyncKnmiMetarFiles(CancellationToken.None);

        await handler.Received(1).HandleFileAsync(Arg.Any<KnmiFileMeta>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncKnmiMetarFiles_WhenRepositoryThrowsRateLimit_DoesNotThrowAndDoesNotCallHandler()
    {
        var logger = Substitute.For<ILogger<DailyFileSyncFeature>>();
        var repo = Substitute.For<IKnmiRepository>();
        var correlation = Substitute.For<ICorrelationIdAccessor>();
        correlation.CorrelationId.Returns(Guid.NewGuid());
        var handler = Substitute.For<INewKnmiFileHandler>();

        repo.GetKnmiMetarFiles(Arg.Any<DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform.Contracts.KnmiFilesParameters>(), Arg.Any<CancellationToken>(), Arg.Any<Guid>())
            .Returns<Task<ICollection<KnmiFileMeta>>>(_ => throw new KnmiRateLimitReachedException());

        var sut = new DailyFileSyncFeature(logger, repo, correlation, handler);

        await sut.SyncKnmiMetarFiles(CancellationToken.None);

        await handler.DidNotReceiveWithAnyArgs().HandleFileAsync(default!, default);
    }

    [Fact]
    public async Task SyncKnmiMetarFiles_WhenRepositoryThrowsApiException_DoesNotThrow()
    {
        var logger = Substitute.For<ILogger<DailyFileSyncFeature>>();
        var repo = Substitute.For<IKnmiRepository>();
        var correlation = Substitute.For<ICorrelationIdAccessor>();
        correlation.CorrelationId.Returns(Guid.NewGuid());
        var handler = Substitute.For<INewKnmiFileHandler>();

        repo.GetKnmiMetarFiles(Arg.Any<DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform.Contracts.KnmiFilesParameters>(), Arg.Any<CancellationToken>(), Arg.Any<Guid>())
            .Returns<Task<ICollection<KnmiFileMeta>>>(_ => throw new DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform.Exceptions.KnmiApiException(System.Net.HttpStatusCode.InternalServerError, "error"));

        var sut = new DailyFileSyncFeature(logger, repo, correlation, handler);

        await sut.SyncKnmiMetarFiles(CancellationToken.None);

        await handler.DidNotReceiveWithAnyArgs().HandleFileAsync(default!, default);
    }
}
