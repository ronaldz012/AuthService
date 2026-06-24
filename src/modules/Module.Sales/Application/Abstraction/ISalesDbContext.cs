using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Module.Sales.Domain;

namespace Module.Sales.Application.Abstraction;

public interface ISaleDbContext
{
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<CashRegisterClosure> CashRegisterClosures { get; set; }
    public DbSet<CashRegisterMovement> CashRegisterMovements { get; set; }
    
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