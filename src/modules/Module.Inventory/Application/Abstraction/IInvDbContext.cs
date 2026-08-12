using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Organization;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Receptions;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Application.Abstraction;

public interface IInvDbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<BranchInventory> BranchInventories { get; set; }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Provider> Providers { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Size> Sizes { get; set; }
    public DbSet<StockReception> StockReceptions { get; set; }
    public DbSet<StockReceptionItem> StockReceptionItems { get; set; }

    public DbSet<StockMovement> StockMovements { get; set; }

    public DbSet<StockTransfer> StockTransfers { get; set; }
    public DbSet<StockTransferItem> StockTransferItems { get; set; }
    
    DatabaseFacade Database { get; }
    
    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        where TEntity : class;

    ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        where TEntity : class;

    EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        where TEntity : class;

    EntityEntry Entry(object entity);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

}