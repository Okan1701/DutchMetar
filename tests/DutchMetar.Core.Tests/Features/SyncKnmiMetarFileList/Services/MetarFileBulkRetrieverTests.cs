using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Services;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DutchMetar.Core.Tests.Features.SyncKnmiMetarFileList.Services;

public class MetarFileBulkRetrieverTests : IDisposable
{
    private readonly MetarFileBulkRetriever _retriever;
    private readonly IKnmiMetarApiClient _knmiMetarApiClient;
    private readonly IMetarProcessor _metarProcessor;
    private readonly DutchMetarContext _context;

    public MetarFileBulkRetrieverTests()
    {
        var builder = new DbContextOptionsBuilder<DutchMetarContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _context = new DutchMetarContext(builder.Options);
        
        _knmiMetarApiClient = Substitute.For<IKnmiMetarApiClient>();
        _metarProcessor = Substitute.For<IMetarProcessor>();
        _retriever = new MetarFileBulkRetriever(_knmiMetarApiClient, _context, Substitute.For<ILogger<MetarFileBulkRetriever>>(), _metarProcessor);
    }
    
    public void Dispose()
    {
        _context.KnmiMetarFiles.RemoveRange(_context.KnmiMetarFiles);
        _context.SaveChanges();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAndSaveKnmiFiles_ApiReturnsMultiPageDataValidMetar_BothPagesSaved()
    {
        const string sampleFileContent =
            "<!-- METAR EHRD 021155Z AUTO 28015G26KT 240V320 9999 FEW011/// SCT014/// BKN017/// 03/01 Q0999 TEMPO 28018G28KT 7000 -SHRASN= -->";
        _knmiMetarApiClient.GetKnmiMetarFileContentAsync(Arg.Any<string>()).ReturnsForAnyArgs(sampleFileContent);
        _knmiMetarApiClient
            .GetMetarFileSummaries(Arg.Is<KnmiFilesParameters>(x => string.IsNullOrEmpty(x.NextPageToken)))
            .Returns(new KnmiListFilesResponse
            {
                StartAfterFilename = "",
                Files = [
                    CreateSampleKnmiFileSummary(),
                    CreateSampleKnmiFileSummary(),
                    CreateSampleKnmiFileSummary()
                ],
                NextPageToken = Guid.NewGuid().ToString(),
                IsTruncated = true
            });
        
        _knmiMetarApiClient
            .GetMetarFileSummaries(Arg.Is<KnmiFilesParameters>(x => !string.IsNullOrEmpty(x.NextPageToken)))
            .Returns(new KnmiListFilesResponse
            {
                StartAfterFilename = "",
                Files = [CreateSampleKnmiFileSummary()]
            });
        
        // Act
        await _retriever.GetAndSaveKnmiFiles(new KnmiFilesParameters(), CancellationToken.None, Guid.Empty);
        
        // Assert
        var savedFiles = await _context.KnmiMetarFiles.ToArrayAsync();
        Assert.Equal(4, savedFiles.Length);
        Assert.True(savedFiles.All(x => x.IsFileProcessed));
        Assert.True(savedFiles.All(x => x.ExtractedRawMetar == "METAR EHRD 021155Z AUTO 28015G26KT 240V320 9999 FEW011/// SCT014/// BKN017/// 03/01 Q0999 TEMPO 28018G28KT 7000 -SHRASN="));
        Assert.True(savedFiles.All(x => !string.IsNullOrWhiteSpace(x.FileName)));
        await _knmiMetarApiClient.Received(2).GetMetarFileSummaries(Arg.Any<KnmiFilesParameters>());
        await _knmiMetarApiClient.Received(4).GetKnmiMetarFileContentAsync(Arg.Any<string>());
        await _metarProcessor.Received(4).ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        
    }
    
    [Fact]
    public async Task GetAndSaveKnmiFiles_MetarExtractionFailed_FileSavedAndNotProcessed()
    {
        _knmiMetarApiClient.GetKnmiMetarFileContentAsync(Arg.Any<string>()).ReturnsForAnyArgs(Guid.NewGuid().ToString());
        _knmiMetarApiClient
            .GetMetarFileSummaries(Arg.Any<KnmiFilesParameters>())
            .Returns(new KnmiListFilesResponse
            {
                StartAfterFilename = "",
                Files = [
                    CreateSampleKnmiFileSummary()
                ],
                IsTruncated = false
            });
        
        // Act
        await _retriever.GetAndSaveKnmiFiles(new KnmiFilesParameters(), CancellationToken.None, Guid.Empty);
        
        // Assert
        var savedFiles = await _context.KnmiMetarFiles.ToArrayAsync();
        Assert.Single(savedFiles);
        Assert.True(savedFiles.All(x => !x.IsFileProcessed));
        Assert.True(savedFiles.All(x => x.ExtractedRawMetar == null));
        Assert.True(savedFiles.All(x => !string.IsNullOrWhiteSpace(x.FileName)));
        await _knmiMetarApiClient.Received(1).GetMetarFileSummaries(Arg.Any<KnmiFilesParameters>());
        await _knmiMetarApiClient.Received(1).GetKnmiMetarFileContentAsync(Arg.Any<string>());
        await _metarProcessor.Received(0).ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task GetAndSaveKnmiFiles_ApiReturnsNoFiles_NothingSaved()
    {
        _knmiMetarApiClient
            .GetMetarFileSummaries(Arg.Any<KnmiFilesParameters>())
            .Returns(new KnmiListFilesResponse
            {
                StartAfterFilename = "",
                Files = [],
                NextPageToken = Guid.NewGuid().ToString(),
                IsTruncated = true
            });
        
        // Act
        await _retriever.GetAndSaveKnmiFiles(new KnmiFilesParameters(), CancellationToken.None, Guid.Empty);
        
        // Assert
        var savedFiles = await _context.KnmiMetarFiles.ToArrayAsync();
        Assert.Empty(savedFiles);
        await _knmiMetarApiClient.Received(1).GetMetarFileSummaries(Arg.Any<KnmiFilesParameters>());
        await _metarProcessor.Received(0).ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private KnmiFileSummary CreateSampleKnmiFileSummary() => new()
    {
        Filename = Guid.NewGuid().ToString(),
        Created = DateTimeOffset.Now,
        LastModified = DateTimeOffset.Now,
    };
}