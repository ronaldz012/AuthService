# Inter-Module Communication

> **Scope:** All modules (Auth, Inventory, Sales)
> **When to use:** Cross-module transactions or coordination between modules

When a use case from one module (e.g., Sales) needs to perform operations in another module (e.g., Inventory) synchronously, we must guarantee data consistency under **ACID** principles (all or nothing).

## 1. Golden Rule: Single AppDbContext

Sales and Inventory share the same `AppDbContext` (defined in `System.Infrastructure`). Both `ISalesDbContext` and `IInvDbContext` resolve to the same `AppDbContext` instance per request. This means changes to entities from either module are tracked by the same change tracker.

```csharp
// Both interfaces resolve to the same AppDbContext instance
services.AddScoped<ISalesDbContext>(sp => sp.GetRequiredService<AppDbContext>());
services.AddScoped<IInvDbContext>(sp => sp.GetRequiredService<AppDbContext>());
```

## 2. Cross-Module Transactions with BeginTransactionAsync

Since Sales and Inventory share the same `AppDbContext`, use EF Core's native transaction API:

```csharp
public class CreateSale(
    ISalesDbContext context,
    IInventoryIntegrationService inventoryService,
    ICurrentUser currentUser,
    ILogger<CreateSale> logger)
{
    public async Task<Result<bool>> Execute(CreateSaleDto dto)
    {
        // ... Previous validations ...

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 1. Call Module B (Inventory) — modifies entities, does NOT save
            var deductResult = await inventoryService.DeductStock(...);
            if (!deductResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return deductResult.Error;
            }

            // 2. Add Sale entities to the same context
            context.Sales.Add(sale);

            // 3. Save all changes in one go (Inventory + Sales)
            await context.SaveChangesAsync();

            // 4. Commit
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, ...);
            return SomeErrors.Failed;
        }
    }
}
```

### Key rules:
- The **integration service** (`DeductStock`) must **NOT** call `SaveChangesAsync()` — it only loads, validates, and modifies entities
- The **use case** owns the transaction and calls `SaveChangesAsync()` once
- Use `await transaction.RollbackAsync()` on business failure before returning, or let `catch` handle it on exception

## 3. Integration Services SaveChangesAsync Policy

| Scenario | SaveChangesAsync? |
|---|---|
| Called from another module's use case (cross-module tx) | **NO** — caller controls transaction |
| Called standalone (e.g., from a controller or seeder) | **YES** — owns its own transaction |

For dual-use integration services, create an overload or pass a `bool saveChanges = true` parameter.

## 4. Auth Module: Global Connection Without Tenant

The Auth module is special: it does **NOT** share the AppDbContext because it manages global data (tenants, users, branches) and must work even before a tenant context exists (e.g., during tenant registration/login).

Auth uses its own fixed connection from `appsettings.json`:

```csharp
services.AddDbContext<AuthDbContext>((sp, options) =>
{
    var connection = configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connection);
});
```

Therefore, Auth **never participates in distributed transactions** with other modules. Its connection is independent and global.
