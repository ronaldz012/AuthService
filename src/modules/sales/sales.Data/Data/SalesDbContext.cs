
using Microsoft.EntityFrameworkCore;
using sales.use.Entities;
using Common.Data;
using Common.Domain;
using Common.Services;

namespace sales.Module.Data;

public class SalesDbContext(DbContextOptions<SalesDbContext> options, ITenantContext tenantContext) : DbContext(options)
{
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrEmpty(tenantContext.Schema))
        {
            modelBuilder.HasDefaultSchema(tenantContext.Schema);
        }


        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);
            entity.HasMany(s => s.SaleItems)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId);
        });
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Obtenemos el TenantId actual desde el servicio de contexto
        var currentTenantId = tenantContext.TenantId ?? throw new InvalidOperationException("Tenant is not set") ;

        // Buscamos todas las entidades que:
        // 1. Están siendo agregadas (Added) o modificadas (Modified)
        // 2. Implementan la interfaz ITenantEntity
        var entries = ChangeTracker.Entries<IMustHaveTenant>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                // Asignamos el TenantId automáticamente al crear
                entry.Entity.TenantId = currentTenantId;
            }
            else if (entry.State == EntityState.Modified)
            {
                // Opcional: Evitar que se cambie el TenantId en ediciones
                entry.Property(x => x.TenantId).IsModified = false;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}



