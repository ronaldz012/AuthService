# Inter-Module Communication: Consistency and Transactionality

When a use case from one module (e.g., Sales) needs to perform operations in another module (e.g., Inventory) synchronously, we must guarantee data consistency under **ACID** principles (all or nothing).

## 1. Golden Rule: Share the Physical Connection (Scoped)

For two modules to join a real database transaction without resorting to heavy distributed transactions, **both DbContexts must use the same physical database connection**.

The tenant connection is resolved through `ITenantConnectionContext`, which exposes a `Connection` property with the connection already configured for the current tenant:

```csharp
// Inventory module — uses tenant.Connection directly
services.AddDbContext<InvDbContext>((sp, options) =>
{
    var tenant = sp.GetRequiredService<ITenantConnectionContext>();
    options.UseNpgsql(tenant.Connection,
        x => x.MigrationsHistoryTable("__EFMigrationsHistory_inventory", tenant.Schema));
});

// Sales module — same physical tenant connection
services.AddDbContext<SalesDbContext>((sp, options) =>
{
    var tenant = sp.GetRequiredService<ITenantConnectionContext>();
    options.UseNpgsql(tenant.Connection,
        x => x.MigrationsHistoryTable("__EFMigrationsHistory_sales", tenant.Schema));
});
```

Both modules receive the same `ITenantConnectionContext` scoped to the request, therefore sharing the same physical connection.

## 2. Orchestration with TransactionScope

To coordinate calls between services from different modules in a linear and clean manner, the Use Case must use `TransactionScope`.

### Safe Workflow Pattern:

```csharp
public class CreateSale(
    ISalesDbContext context,
    IInventoryIntegrationService inventoryService,
    ITenantConnectionContext tenantConnection)
{
    public async Task<Result<bool>> Execute(CreateSaleDto dto)
    {
        // ... Previous validations ...

        // 1. Declare the global transaction scope
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        // 2. Ensure the shared connection is open
        var conn = tenantConnection.Connection;
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        try
        {
            // 3. Call Module B (Inventory) -> Modifies and internally calls SaveChanges
            var deductResult = await inventoryService.DeductStock(...);
            if (!deductResult.IsSuccess)
                return deductResult.Error;

            // 4. Call Module A (Sales) -> Modifies and internally calls SaveChanges
            context.Sales.Add(sale);
            await context.SaveChangesAsync();

            // 5. Commit global changes in PostgreSQL
            scope.Complete();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database transaction failed.");
            return new Error(ErrorCode.DatabaseError, "An unexpected database error occurred.");
        }
    }
}
```

## 3. How Automatic Rollback Works

- **Successful flow:** `scope.Complete()` is called. When the `using` block closes, the physical transaction issues a **COMMIT** in the database.
- **Business failure:** The flow returns early (e.g., `return deductResult.Error;`). When the method exits, the `using` disposes the `scope`. .NET detects that `.Complete()` was not called and executes a **ROLLBACK** in PostgreSQL.
- **Exception:** Execution jumps directly to the `catch` block without calling `.Complete()`. When the `scope` is disposed, a **ROLLBACK** is executed immediately.

## 4. Auth Module: Global Connection Without Tenant

The Auth module is special: it does **NOT** use `ITenantConnectionContext` because it manages global data (tenants, users, branches) and must work even before a tenant context exists (e.g., during tenant registration/login).

Auth uses its own fixed connection from `appsettings.json`:

```csharp
services.AddDbContext<AuthDbContext>((sp, options) =>
{
    var connection = configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connection,
        x => x.MigrationsHistoryTable("__EFMigrationsHistory_shared", null));
});
```

Therefore, Auth **never participates in distributed transactions** with other modules. Its connection is independent and global.
