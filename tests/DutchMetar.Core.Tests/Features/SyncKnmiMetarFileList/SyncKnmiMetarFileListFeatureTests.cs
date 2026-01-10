using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.SyncKnmiMetarFileList;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DutchMetar.Core.Tests.Features.SyncKnmiMetarFileList;

public class SyncKnmiMetarFileListFeatureTests : IDisposable
{
    private readonly SyncKnmiMetarFileListFeature _feature;
    private readonly DutchMetarContext _context;
    private readonly IMetarFileBulkRetriever _mockBulkRetriever;
    
    public SyncKnmiMetarFileListFeatureTests()
    {
        var builder = new DbContextOptionsBuilder<DutchMetarContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        _mockBulkRetriever = Substitute.For<IMetarFileBulkRetriever>();
        _context = new DutchMetarContext(builder.Options);
        
        _feature = new SyncKnmiMetarFileListFeature(_context, Substitute.For<ILogger<SyncKnmiMetarFileListFeature>>(), _mockBulkRetriever);
    }
    
    public void Dispose()
    {
        _context.KnmiMetarFiles.RemoveRange(_context.KnmiMetarFiles);
        _context.SaveChanges();
        _context.Dispose();
    }

    [Fact]
    public async Task SyncKnmiMetarFiles_ExistingSavedFiles_CorrectRequestParamsUsed()
    {
        // Arrange
        var savedMetarFiles = new List<KnmiMetarFile>();
        for (var i = 0; i < 10; i++)
        {
            savedMetarFiles.Add(new()
            {
                FileCreatedAt = DateTime.UtcNow.AddYears(-i).AddDays(0 * -1),
                FileLastModifiedAt = DateTime.UtcNow.AddYears(-i).AddDays(-1),
                FileName = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow.AddYears(-i).AddDays(-1)
            });
        }
        await _context.KnmiMetarFiles.AddRangeAsync(savedMetarFiles);
        await _context.SaveChangesAsync();
        
        
        _mockBulkRetriever.GetAndSaveKnmiFiles(Arg.Any<KnmiFilesParameters>(), CancellationToken.None, Guid.NewGuid()).Throws<ArgumentException>();
        _mockBulkRetriever.GetAndSaveKnmiFiles(
            Arg.Is<KnmiFilesParameters>(x => 
                x.Begin == savedMetarFiles.First().FileCreatedAt &&
                x.Sorting == "desc" &&
                x.OrderBy == "created" &&
                x.End == null),
            CancellationToken.None, 
            Guid.NewGuid()).Returns(Task.CompletedTask);
        _mockBulkRetriever.GetAndSaveKnmiFiles(
            Arg.Is<KnmiFilesParameters>(x => 
                x.Begin == new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) &&
                x.End == savedMetarFiles.Last().FileLastModifiedAt &&
                x.Sorting == "desc" &&
                x.OrderBy == "created"),
            CancellationToken.None, 
            Guid.NewGuid()).Returns(Task.CompletedTask);
        
        // Act
        await _feature.SyncKnmiMetarFiles();
        
        // Assert
        await _mockBulkRetriever
            .Received(2)
            .GetAndSaveKnmiFiles(Arg.Any<KnmiFilesParameters>(), Arg.Any<CancellationToken>(), Arg.Any<Guid>());
    }
}