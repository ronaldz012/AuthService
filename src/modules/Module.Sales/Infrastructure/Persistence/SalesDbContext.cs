using Common.Contracts.authentication;
using Common.Domain;
using Microsoft.EntityFrameworkCore;
using Module.Sales.Application.Abstraction;
using Module.Sales.Domain;

namespace Module.Sales.Infrastructure.Persistence;

public class SalesDbContext(DbContextOptions<SalesDbContext> options, ITenantConnectionContext tenantConnectionContext) : DbContext(options), ISalesDbContext
{
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<CashRegisterClosure> CashRegisterClosures { get; set; }
    public DbSet<CashRegisterMovement> CashRegisterMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrEmpty(tenantConnectionContext.Schema))
        {
            modelBuilder.HasDefaultSchema(tenantConnectionContext.Schema);
        }


        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
            entity.HasMany(s => s.SaleItems)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId);
        });
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
        });

        modelBuilder.Entity<CashRegisterClosure>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
            entity.HasMany(c => c.Movements)
                .WithOne(m => m.CashRegisterClosure)
                .HasForeignKey(m => m.CashRegisterClosureId);

            entity.HasMany(c => c.Sales)
                .WithOne(s => s.CashRegisterClosure)
                .HasForeignKey(s => s.CashRegisterClosureId);
        });

        modelBuilder.Entity<CashRegisterMovement>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Obtenemos el TenantId actual desde el servicio de contexto
        var currentTenantId = tenantConnectionContext.TenantId ?? throw new InvalidOperationException("Tenant is not set") ;

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



