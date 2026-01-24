using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;
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
    public async Task GetAndSaveKnmiFiles_EhggMetarSample_NoParseExceptionsThrown()
    {
        const string fileContent =
            "<?xml version=\"1.0\" ?>\n<iwxxm:SPECI automatedStation=\"false\" gml:id=\"uuid.d68ee3a3-3bb9-4ddc-8420-cca74d47c173\" permissibleUsage=\"OPERATIONAL\" reportStatus=\"NORMAL\" translatedBulletinID=\"\" translatedBulletinReceptionTime=\"2023-01-01T22:13:19Z\" translationCentreDesignator=\"EHGG\" translationCentreName=\"MetConsole\" translationTime=\"2023-01-01T22:13:19Z\" xmlns:aixm=\"http://www.aixm.aero/schema/5.1.1\" xmlns:gml=\"http://www.opengis.net/gml/3.2\" xmlns:iwxxm=\"http://icao.int/iwxxm/3.0\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://icao.int/iwxxm/3.0 http://schemas.wmo.int/iwxxm/3.0/iwxxm.xsd\nhttp://def.wmo.int/metce/2013 http://schemas.wmo.int/metce/1.2/metce.xsd\">\n    <!-- SPECI EHGG 012214Z AUTO 14004KT 9999 DZ BKN023 OVC026 09/08 Q1011= -->\n    <iwxxm:issueTime>\n        <gml:TimeInstant gml:id=\"uuid.837bea47-899e-4a89-8cc0-09dcaedba2c8\">\n            <gml:timePosition>2023-01-01T22:14:00Z</gml:timePosition>\n        </gml:TimeInstant>\n    </iwxxm:issueTime>\n    <iwxxm:aerodrome>\n        <aixm:AirportHeliport ";
        _knmiMetarApiClient.GetKnmiMetarFileContentAsync(Arg.Any<string>()).ReturnsForAnyArgs(fileContent);
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
        Assert.True(savedFiles.All(x => x.IsFileProcessed));
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
        await _metarProcessor.Received(4).ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
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
        await _metarProcessor.Received(0).ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
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
        await _metarProcessor.Received(0).ProcessRawMetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    private KnmiFileSummary CreateSampleKnmiFileSummary() => new()
    {
        Filename = Guid.NewGuid().ToString(),
        Created = DateTimeOffset.Now,
        LastModified = DateTimeOffset.Now,
    };
}