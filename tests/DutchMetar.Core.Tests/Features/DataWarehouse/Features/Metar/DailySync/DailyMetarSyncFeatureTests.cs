using DutchMetar.Core.Features.DataWarehouse.Features.Metar.DailySync;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Infrastructure.Accessors;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Metar.DailySync;

public class DailyMetarSyncFeatureTests
{
    private readonly IKnmiRepository _repo;
    private readonly IMetarFileHandler _handler;
    private readonly DailyMetarSyncFeature _feature;

    public DailyMetarSyncFeatureTests()
    {
        var logger = Substitute.For<ILogger<DailyMetarSyncFeature>>();
        _repo = Substitute.For<IKnmiRepository>();
        var correlation = Substitute.For<ICorrelationIdAccessor>();
        correlation.CorrelationId.Returns(Guid.NewGuid());
        _handler = Substitute.For<IMetarFileHandler>();
        _feature = new DailyMetarSyncFeature(logger, _repo, correlation, _handler);
    }

    [Fact]
    public async Task SyncKnmiMetarFiles_WhenRepositoryReturnsFiles_CallsHandlerForEach()
    {
        _repo.GetKnmiMetarFiles(Arg.Any<KnmiFilesParameters>(), Arg.Any<CancellationToken>(), Arg.Any<Guid>())
            .Returns(Task.FromResult<ICollection<KnmiFileMeta>>(new List<KnmiFileMeta> { new KnmiFileMeta { FileName = "file1", CreatedOn = DateTimeOffset.UtcNow } }));

        await _feature.SyncKnmiMetarFiles(CancellationToken.None);

        await _handler.Received(1).HandleFileAsync(Arg.Any<KnmiFileMeta>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncKnmiMetarFiles_WhenRepositoryThrowsRateLimit_DoesNotThrowAndDoesNotCallHandler()
    {
        _repo.GetKnmiMetarFiles(Arg.Any<KnmiFilesParameters>(), Arg.Any<CancellationToken>(), Arg.Any<Guid>())
            .Returns<Task<ICollection<KnmiFileMeta>>>(_ => throw new KnmiRateLimitReachedException());

        await _feature.SyncKnmiMetarFiles(CancellationToken.None);

        await _handler.DidNotReceiveWithAnyArgs().HandleFileAsync(null!, CancellationToken.None);
    }

    [Fact]
    public async Task SyncKnmiMetarFiles_WhenRepositoryThrowsApiException_DoesNotThrow()
    {
        _repo.GetKnmiMetarFiles(Arg.Any<KnmiFilesParameters>(), Arg.Any<CancellationToken>(), Arg.Any<Guid>())
            .Returns<Task<ICollection<KnmiFileMeta>>>(_ => throw new KnmiApiException(System.Net.HttpStatusCode.InternalServerError, "error"));

        await _feature.SyncKnmiMetarFiles(CancellationToken.None);

        await _handler.DidNotReceiveWithAnyArgs().HandleFileAsync(null!, CancellationToken.None);
    }
}
