using Microsoft.EntityFrameworkCore;
using shared.Module.Entities;

namespace shared.Module.Data;

public class SharedDbContext(DbContextOptions<SharedDbContext> options) : DbContext(options)
{
    public DbSet<Entities.Module> Modules { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasOne(f => f.Module)
                .WithMany(m => m.Features)
                .HasForeignKey(f => f.ModuleId);
        });
    }
}