using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Features.ReprocessFailedFiles;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
using DutchMetar.Core.Infrastructure.Accessors;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.ReprocessFailedFiles;

public class ReprocessFailedFilesFeatureTests : TestsWithContext
{
    private readonly ReprocessFailedFilesFeature _feature;
    private readonly IMetarProcessor _metarProcessor;

    public ReprocessFailedFilesFeatureTests()
    {
        _metarProcessor = Substitute.For<IMetarProcessor>();
        _feature = new(Context, Substitute.For<ILogger<ReprocessFailedFilesFeature>>(), _metarProcessor, new SimpleCorrelationIdAccessor());
    }

    [Fact]
    public async Task ReprocessFailedFilesAsync_UnprocessedFilesPresent_NoUnprocessedFilesLeft()
    {
        // Create 2000 files to verify the batching works
        for (int i = 0; i < 2000; i++)
        {
            Context.KnmiMetarFiles.Add(new KnmiMetarFile
            {
                FileCreatedAt = DateTime.UtcNow.AddDays(-i),
                FileLastModifiedAt = DateTime.UtcNow.AddDays(-i),
                FileName = Guid.NewGuid().ToString(),
                ExtractedRawMetar = Guid.NewGuid().ToString(),
                IsFileProcessed = false
            });
        }
        await Context.SaveChangesAsync();
        _metarProcessor
            .ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(),
                CancellationToken.None)
            .ReturnsForAnyArgs(Task.CompletedTask);
        
        // Act
        await _feature.ReprocessFailedFilesAsync(CancellationToken.None);
        
        // Assert
        Assert.Empty(Context.KnmiMetarFiles.Where(x => !x.IsFileProcessed));
        await _metarProcessor.Received(2000).ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), CancellationToken.None);
    }
    
    [Fact]
    public async Task ReprocessFailedFilesAsync_UnprocessableFile_NoInfiniteLoop()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        Context.KnmiMetarFiles.Add(new KnmiMetarFile
        {
            FileCreatedAt = DateTime.UtcNow,
            FileLastModifiedAt = DateTime.UtcNow,
            FileName = Guid.NewGuid().ToString(),
            ExtractedRawMetar = Guid.NewGuid().ToString(),
            IsFileProcessed = false
        });
        await Context.SaveChangesAsync(cancellationTokenSource.Token);
        _metarProcessor
            .ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(),
                CancellationToken.None)
            .ThrowsAsyncForAnyArgs(new MetarParseException(string.Empty));
        
        // Act
        cancellationTokenSource.CancelAfter(10000);
        await _feature.ReprocessFailedFilesAsync(cancellationTokenSource.Token);
        
        // Assert
        Assert.Single(Context.KnmiMetarFiles.Where(x => !x.IsFileProcessed));
        cancellationTokenSource.Dispose();
    }
    
    [Fact]
    public async Task ReprocessFailedFilesAsync_EmptyMetarValue_NotProcessed()
    {
        Context.KnmiMetarFiles.Add(new KnmiMetarFile
        {
            FileCreatedAt = DateTime.UtcNow,
            FileLastModifiedAt = DateTime.UtcNow,
            FileName = Guid.NewGuid().ToString(),
            IsFileProcessed = false
        });
        await Context.SaveChangesAsync();
        _metarProcessor
            .ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(),
                CancellationToken.None)
            .ReturnsForAnyArgs(Task.CompletedTask);
        
        // Act
        await _feature.ReprocessFailedFilesAsync();
        
        // Assert
        Assert.Single(Context.KnmiMetarFiles.Where(x => !x.IsFileProcessed));
        await _metarProcessor.Received(0).ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), CancellationToken.None);
    }
}