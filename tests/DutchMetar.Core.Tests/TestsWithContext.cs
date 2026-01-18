using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Core.Tests;

public abstract class TestsWithContext : IDisposable
{
    protected DutchMetarContext Context { get; private set; }

    protected TestsWithContext()
    {
        var builder = new DbContextOptionsBuilder<DutchMetarContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        Context = new DutchMetarContext(builder.Options);
    }

    public virtual void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}