using DutchMetar.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DutchMetar.Core.Infrastructure.Data;

public class DutchMetarContext : DbContext
{
    public const string ConnectionStringKey = "DutchMetarMssql";
    
    public DbSet<Airport> Airports { get; set; }
    
    public DbSet<Metar> Metars { get; set; }
    
    public DbSet<MetarImportResult> MetarImportResults { get; set; }

    public DutchMetarContext()
    {
    }

    public DutchMetarContext(DbContextOptions options) : base(options)
    {
    }

    public override int SaveChanges()
    {
        SetOrUpdateTrackedEntityProperties();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetOrUpdateTrackedEntityProperties();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        SetOrUpdateTrackedEntityProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        SetOrUpdateTrackedEntityProperties();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    
    private void SetOrUpdateTrackedEntityProperties()
    {
        foreach (var changedEntity in ChangeTracker.Entries())
        {
            if (changedEntity.Entity is Entity entity)
            {
                switch (changedEntity.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        entity.LastUpdatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Modified:
                        Entry(entity).Property(x => x.CreatedAt).IsModified = false;
                        entity.LastUpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}