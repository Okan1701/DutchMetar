using DutchMetar.Core.Features.DataWarehouse.Features.Taf.DailySync;
using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Infrastructure.Accessors;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Taf.DailySync;

public class DailyTafSyncFeatureTests
{
    private readonly IKnmiRepository _repository = Substitute.For<IKnmiRepository>();
    private readonly ITafFileHandler _handler = Substitute.For<ITafFileHandler>();
    private readonly DailyTafSyncFeature _feature;

    public DailyTafSyncFeatureTests()
    {
        var correlation = Substitute.For<ICorrelationIdAccessor>();
        correlation.CorrelationId.Returns(Guid.NewGuid());
        _feature = new DailyTafSyncFeature(
            Substitute.For<ILogger<DailyTafSyncFeature>>(),
            _repository,
            correlation,
            _handler);
    }

    [Fact]
    public async Task SyncKnmiTafFiles_RetrievesAndProcessesFilesForThePrevious24Hours()
    {
        KnmiFilesParameters? capturedParameters = null;
        _repository.GetKnmiTafFiles(
                Arg.Do<KnmiFilesParameters>(parameters => capturedParameters = parameters),
                Arg.Any<CancellationToken>(),
                Arg.Any<Guid>())
            .Returns(Task.FromResult<ICollection<KnmiFileMeta>>(new List<KnmiFileMeta>
            {
                new() { FileName = "taf-file", CreatedOn = DateTimeOffset.UtcNow }
            }));

        await _feature.SyncKnmiTafFiles();

        Assert.NotNull(capturedParameters);
        Assert.NotNull(capturedParameters.Begin);
        Assert.NotNull(capturedParameters.End);
        Assert.Equal(TimeSpan.FromDays(1), capturedParameters.End.Value - capturedParameters.Begin.Value);
        Assert.Equal("desc", capturedParameters.Sorting);
        Assert.Equal("created", capturedParameters.OrderBy);
        await _handler.Received(1).HandleFileAsync(
            Arg.Is<KnmiFileMeta>(file => file.FileName == "taf-file"),
            Arg.Any<CancellationToken>());
    }
}
