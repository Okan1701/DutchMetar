using DutchMetar.Core.Features.SyncKnmiMetarFileList;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Interfaces;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.SyncKnmiMetarFileList;

public class SyncKnmiMetarFileListFeatureTests : IDisposable
{
    private readonly SyncKnmiMetarFileListFeature _feature;
    private readonly DutchMetarContext _context;
    private readonly IKnmiMetarApiClient _mockedApiClient;
    private const string SampleFileName = "A_LANL80EHRD021209_C_EHRD_020126120929";
    
    public SyncKnmiMetarFileListFeatureTests()
    {
        var builder = new DbContextOptionsBuilder<DutchMetarContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        _mockedApiClient = Substitute.For<IKnmiMetarApiClient>();
        _context = new DutchMetarContext(builder.Options);
        
        _feature = new SyncKnmiMetarFileListFeature(_mockedApiClient, _context, Substitute.For<ILogger<SyncKnmiMetarFileListFeature>>());
    }
    
    public void Dispose()
    {
        _context.KnmiMetarFiles.RemoveRange(_context.KnmiMetarFiles);
        _context.SaveChanges();
        _context.Dispose();
    }
    
    [Fact]
    public async Task SyncKnmiFiles_ApiReturnsSInglePage_ApiFilesSavedToDb()
    {
        _mockedApiClient.GetMetarFileSummaries(Arg.Any<KnmiFilesParameters>(), Arg.Any<CancellationToken>())
            .Returns(new KnmiListFilesResponse
            {
                IsTruncated = false,
                Files =
                [
                    new KnmiFileSummary
                    {
                        Filename = SampleFileName,
                        Created = DateTime.Today.AddDays(-1),
                    }
                ],
                StartAfterFilename = "",
                MaxResults = 10,
                ResultCount = 1
            });
        
        await _feature.SyncKnmiFiles();

        var allSavedFiles = await _context.KnmiMetarFiles.ToArrayAsync();
        Assert.Single(allSavedFiles);
        await _mockedApiClient.ReceivedWithAnyArgs(1).GetMetarFileSummaries(Arg.Any<KnmiFilesParameters>());
    }
    
    [Fact]
    public async Task SyncKnmiFiles_ApiReturnsMultiplePages_FilesSavedToDb()
    {
        var page1ApiResponse = new KnmiListFilesResponse
        {
            Files = new List<KnmiFileSummary>(),
            IsTruncated = true,
            MaxResults = 10,
            NextPageToken = Guid.NewGuid().ToString(),
            ResultCount = 10,
            StartAfterFilename = ""
        };
        for (int i = 0; i <= 10; i++)
        {
            page1ApiResponse.Files.Add(new()
            {
                Filename = Guid.NewGuid().ToString(),
                Created = DateTime.Today.AddDays(i * -1),
                LastModified = DateTime.Today.AddDays(i * -1)
            });
        }
        
        var page2ApiResponse = new KnmiListFilesResponse
        {
            Files = new List<KnmiFileSummary>(),
            IsTruncated = false,
            MaxResults = 10,
            NextPageToken = Guid.NewGuid().ToString(),
            ResultCount = 5,
            StartAfterFilename = ""
        };
        for (int i = 0; i <= 5; i++)
        {
            page2ApiResponse.Files.Add(new()
            {
                Filename = Guid.NewGuid().ToString(),
                Created = DateTime.Today.AddDays(i * -1 - 5),
                LastModified = DateTime.Today.AddDays(i * -1 - 5)
            });
        }
        
        _mockedApiClient.GetMetarFileSummaries(Arg.Is<KnmiFilesParameters>(x => x.NextPageToken == null), Arg.Any<CancellationToken>())
            .Returns(page1ApiResponse);
        
        _mockedApiClient.GetMetarFileSummaries(Arg.Is<KnmiFilesParameters>(x => x.NextPageToken != null), Arg.Any<CancellationToken>())
            .Returns(page2ApiResponse);
        
        await _feature.SyncKnmiFiles();

        var allSavedFiles = await _context.KnmiMetarFiles.ToArrayAsync();
        Assert.Equal(page1ApiResponse.Files.Count + page2ApiResponse.Files.Count, allSavedFiles.Length);
        await _mockedApiClient.ReceivedWithAnyArgs(2).GetMetarFileSummaries(Arg.Any<KnmiFilesParameters>());
    }
}