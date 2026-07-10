using Common.Contracts.authentication;
using Common.Domain;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Organization;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Receptions;
using Module.Inventory.Domain.Transfers;
using Npgsql;

namespace Module.Inventory.Infrastructure.Persistence;

public class InvDbContext(DbContextOptions<InvDbContext> options, ITenantConnectionContext tenantConnectionContext ) : DbContext(options), IInvDbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<BranchInventory> BranchInventories { get; set; }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Provider> Providers { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<StockReception> StockReceptions { get; set; }
    public DbSet<StockReceptionItem> StockReceptionItems { get; set; }

    public DbSet<StockMovement> StockMovements { get; set; }

    public DbSet<StockTransfer> StockTransfers { get; set; }
    public DbSet<StockTransferItem> StockTransferItems { get; set; }
    


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        
        if (!string.IsNullOrEmpty(tenantConnectionContext.Schema))
        {
            modelBuilder.HasDefaultSchema(tenantConnectionContext.Schema);
        }
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(tenantConnectionContext.Schema);
  
        modelBuilder.Entity<Product>(entity =>
            {
                entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
                entity.HasQueryFilter(x => x.DeletedAt == null);

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
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
            entity.HasQueryFilter(x => x.DeletedAt == null);

            entity.HasMany(pv => pv.BranchInventories)
                .WithOne(inv => inv.ProductVariant)
                .HasForeignKey(inv => inv.ProductVariantId);

            entity.HasMany(pv => pv.StockMovements)
                .WithOne(inv => inv.ProductVariant)
                .HasForeignKey(inv => inv.ProductVariantId);
            entity.HasMany(pv => pv.TransferItems)
                .WithOne(ti => ti.ProductVariant)
                .HasForeignKey(ti => ti.ProductVariantId);
            
            entity.HasOne(pv => pv.Color)
                .WithMany(c => c.ProductVariant)
                .HasForeignKey(pv => pv.ColorId);
        });
        modelBuilder.Entity<BranchInventory>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
        });
        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
        });
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
        });
        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
        });
        //RECEPTIONS
        modelBuilder.Entity<StockReception>(entity =>
            {
                entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);

                entity.HasMany(r => r.Items)
                    .WithOne(i => i.StockReception)
                    .HasForeignKey(i => i.StockReceptionId);

            }
        );

        modelBuilder.Entity<StockReceptionItem>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);

            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);

            entity.HasOne(ri => ri.ProductVariant)
                .WithMany(pv => pv.StockReceptionItems)
                .HasForeignKey(pv => pv.ProductVariantId);
        });

        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
        });
        modelBuilder.Entity<StockTransferItem>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
        });

    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentTenantId = tenantConnectionContext.TenantId ?? throw new InvalidOperationException("Tenant is not set") ;

        var entries = ChangeTracker.Entries<IMustHaveTenant>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = currentTenantId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.TenantId).IsModified = false;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> ReserveBrandCounter(Guid brandId, string prefix)
    {
        var schema = tenantConnectionContext.Schema;
        var sql = $"""
                   UPDATE "{schema}"."Brands"
                   SET "ProductCounter" = "ProductCounter" + 1
                   WHERE "Id" = @id
                   RETURNING "ProductCounter"
                   """;

        var result = await Database
            .SqlQueryRaw<int>(sql, new NpgsqlParameter("id", brandId))
            .ToListAsync();

        return $"{prefix}-{result[0]}";
    }

    public async Task<string> ReserveVariantCounter(Guid productId, string productCode)
    {
        var schema = tenantConnectionContext.Schema;
        var sql = $"""
                   UPDATE "{schema}"."Products"
                   SET "ProductVariantCounter" = "ProductVariantCounter" + 1
                   WHERE "Id" = @id
                   RETURNING "ProductVariantCounter"
                   """;

        var result = await Database
            .SqlQueryRaw<int>(sql, new NpgsqlParameter("id", productId))
            .ToListAsync();

        return $"{productCode}-{result[0].ToString().PadLeft(3, '0')}";
    }
}