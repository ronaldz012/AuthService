using Common.Contracts.authentication;
using Common.Domain;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Organization;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Receptions;
using Module.Inventory.Domain.Transfers;
using Module.Sales.Application.Abstraction;
using Module.Sales.Domain;
using Npgsql;

namespace System.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantConnectionContext tenantConnectionContext)
    : DbContext(options), ISalesDbContext, IInvDbContext
{
    // ===== Sales =====
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<CashRegisterClosure> CashRegisterClosures { get; set; }
    public DbSet<CashRegisterMovement> CashRegisterMovements { get; set; }

    // ===== Inventory =====
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
        base.OnModelCreating(modelBuilder);

        // ===== Sales entity configuration =====
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

        // ===== Inventory entity configuration =====
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
        });
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
        modelBuilder.Entity<StockReception>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantConnectionContext.TenantId);
            entity.HasMany(r => r.Items)
                .WithOne(i => i.StockReception)
                .HasForeignKey(i => i.StockReceptionId);
        });
        modelBuilder.Entity<StockReceptionItem>(entity =>
        {
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
        var currentTenantId = tenantConnectionContext.TenantId
            ?? throw new InvalidOperationException("Tenant is not set");

        var entries = ChangeTracker.Entries<IMustHaveTenant>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = currentTenantId;
            else if (entry.State == EntityState.Modified)
                entry.Property(x => x.TenantId).IsModified = false;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<string> ReserveBrandCounter(Guid brandId, string prefix)
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

    public virtual async Task<string> ReserveVariantCounter(Guid productId, string productCode)
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
