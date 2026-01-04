using DutchMetar.Core.Features.SyncKnmiMetarFileList;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Contracts;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Interfaces;
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
        var metarProcessor = Substitute.For<IMetarProcessor>();
        
        _feature = new SyncKnmiMetarFileListFeature(_mockedApiClient, _context, Substitute.For<ILogger<SyncKnmiMetarFileListFeature>>(), metarProcessor);
    }
    
    public void Dispose()
    {
        _context.KnmiMetarFiles.RemoveRange(_context.KnmiMetarFiles);
        _context.SaveChanges();
        _context.Dispose();
    }
    
}