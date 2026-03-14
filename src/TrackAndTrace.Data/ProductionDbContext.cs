using Microsoft.EntityFrameworkCore;
using TrackAndTrace.Data.Entities;

namespace TrackAndTrace.Data;

public sealed class ProductionDbContext : DbContext
{
    /// <summary>
    /// Passes EF Core options through to the base DbContext.
    /// </summary>
    public ProductionDbContext(DbContextOptions<ProductionDbContext> options)
        : base(options) { }

    public DbSet<ProductionEntry> ProductionEntries => Set<ProductionEntry>();

    /// <summary>
    /// Configures column constraints and indexes for the production entries table.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductionEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(64);
            entity.Property(e => e.StationId).IsRequired().HasMaxLength(32);
            entity.Property(e => e.PluginSummary).HasMaxLength(512);
            entity.HasIndex(e => e.SerialNumber);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
