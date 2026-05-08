using Common.Data;
using Common.Domain;
using Inventory.Data.Entities.Inventory;
using Inventory.Data.Entities.Organization;
using Inventory.Data.Entities.Products;
using Inventory.Data.Entities.Receptions;
using Inventory.Data.Entities.Transfers;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data;

public class InvDbContext(DbContextOptions<InvDbContext> options, ITenantContext tenantContext ) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<BranchInventory> BranchInventories { get; set; }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Provider> Providers { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<StockReception> StockReceptions { get; set; }
    public DbSet<StockReceptionItem> StockReceptionItems { get; set; }

    public DbSet<StockMovement> StockMovements { get; set; }

    public DbSet<StockTransfer> StockTransfers { get; set; }
    public DbSet<StockTransferItem> StockTransferItems { get; set; }
    


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        
        if (!string.IsNullOrEmpty(tenantContext.Schema))
        {
            modelBuilder.HasDefaultSchema(tenantContext.Schema);
        }
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(tenantContext.Schema);
  
        modelBuilder.Entity<Product>(entity =>
            {
                entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);

                entity.HasMany(product => product.ProductVariants)
                    .WithOne(variant => variant.Product)
                    .HasForeignKey(variant => variant.ProductId);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId);

                entity.HasOne(p => p.Brand)
                    .WithMany(b => b.Products)
                    .HasForeignKey(p => p.BrandId);
            }
        );
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);

            entity.HasMany(pv => pv.BranchInventories)
                .WithOne(inv => inv.ProductVariant)
                .HasForeignKey(inv => inv.ProductVariantId);

            entity.HasMany(pv => pv.StockMovements)
                .WithOne(inv => inv.ProductVariant)
                .HasForeignKey(inv => inv.ProductVariantId);
            entity.HasMany(pv => pv.TransferItems)
                .WithOne(ti => ti.ProductVariant)
                .HasForeignKey(ti => ti.ProductVariantId);
        });
        modelBuilder.Entity<BranchInventory>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);
        });
        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);
        });
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);
        });
        //RECEPTIONS
        modelBuilder.Entity<StockReception>(entity =>
            {
                entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);

                entity.HasMany(r => r.Items)
                    .WithOne(i => i.StockReception)
                    .HasForeignKey(i => i.StockReceptionId);

            }
        );

        modelBuilder.Entity<StockReceptionItem>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);

            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);

            entity.HasOne(ri => ri.ProductVariant)
                .WithMany(pv => pv.StockReceptionItems)
                .HasForeignKey(pv => pv.ProductVariantId);
        });
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);

            entity.HasOne(sm => sm.StockTransfer)
                .WithMany(st => st.StockMovements)
                .HasForeignKey(sm => sm.stockTransferId)
                .IsRequired(false);
        });

        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);
        });
        modelBuilder.Entity<StockTransferItem>(entity =>
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