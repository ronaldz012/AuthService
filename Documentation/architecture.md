# DriveCoreSystem — Architecture

## Overview

Modular monolith with pragmatic Clean Architecture in .NET 8/9. Single deployment (System.Api) with separated bounded contexts in independent modules.

```
┌─────────────────────────────────────────────────────┐
│  System.Api (Composition Root / Host)               │
│  ┌──────────────────────────────────────────────┐   │
│  │         System.Infrastructure                │   │
│  │         ┌── AppDbContext ──────────────────┐  │   │
│  │         │ DbSets: Sales + Inventory        │  │   │
│  │         └──────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────┘   │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐            │
│  │  Auth    │ │Inventory │ │  Sales   │            │
│  │  Module  │ │  Module  │ │  Module  │            │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘            │
│       └──────┬──────┴──────┬────┘                   │
│              │    Common    │                        │
│              └─────────────┘                        │
└─────────────────────────────────────────────────────┘
```

---

## Common (Shared Kernel)

Cross-cutting library with no dependencies toward modules.

| Folder | Content |
|---------|-----------|
| `Contracts/` | Inter-module communication interfaces (`IInventoryIntegrationService`, `IBranchService`, `IUserIntegrationService`, `ITenantConnectionContext`) |
| `Contracts/Seeder/` | `IDataSeeder` + `DatabaseSeeder` (aggregator) |
| `Domain/` | Base interfaces: `IMustHaveTenant`, `ISoftDelete`, `ICreatedAt`, `IUpdatedAt`, `ICreatedBy`, `IUpdatedBy` |
| `Utilities/` | `Result<T>` (Railway Oriented Programming), pagination, dynamic filters over `IQueryable` |
| `Services/` | `IEmailService`, settings (`TokenSettings`, `SmtpSettings`, `TenantOptions`) |

---

## System.Infrastructure (Unified DbContext)

Shared project containing the **single DbContext** (AppDbContext) for Sales + Inventory. Auth has its own DbContext (global, non multi-tenant).

### Responsibilities

- `Persistence/AppDbContext` — implements `ISalesDbContext` and `IInvDbContext`
- `Persistence/AppDbContextFactory` — design-time factory for migrations
- `SystemInfrastructureDependencyInjection` — registers `AppDbContext` and forwarding to both interfaces

```
AppDbContext : ISalesDbContext, IInvDbContext
├── Sales: Sale, SaleItem, CashRegisterClosure, CashRegisterMovement
└── Inventory: Product, ProductVariant, BranchInventory, Category, Provider,
               Brand, Color, StockReception, StockReceptionItem,
               StockMovement, StockTransfer, StockTransferItem
```

### Configuration

Single `AddDbContext<AppDbContext>` with `ITenantConnectionContext.Connection` (same physical Npgsql connection for the entire request). Single `__EFMigrationsHistory` table.

```csharp
services.AddDbContext<AppDbContext>((sp, options) =>
{
    var tenant = sp.GetRequiredService<ITenantConnectionContext>();
    options.UseNpgsql(tenant.Connection);
});

services.AddScoped<ISalesDbContext>(sp => sp.GetRequiredService<AppDbContext>());
services.AddScoped<IInvDbContext>(sp => sp.GetRequiredService<AppDbContext>());
```

---

## Modules

Each module is an independent project with 3 layers. **They never reference each other**, only Common and (where applicable) `System.Infrastructure`.

### Internal structure

```
Module.X/
├── Domain/                 # Entities and enums, zero dependencies
├── Application/
│   ├── Abstraction/        # DbContext interfaces and external service contracts
│   └── UseCases/           # Use cases (one class per operation)
└── Infrastructure/
    ├── Seeder/             # IDataSeeder implementations
    └── Services/           # Implementations of Common contracts
```

> Note: Persistence no longer lives in modules. The unified DbContext is in `System.Infrastructure`.

### Dependency rules

- **Domain** — zero external dependencies
- **Application** → depends on `Domain` + `Common.Contracts`
- **Infrastructure** → depends on `Application` + `Common`
- Modules **never reference each other**

### Module.Auth

- Users, roles, authentication (JWT + Google OAuth), tenants, branches, features/permissions
- Own DbContext: `AuthDbContext` (global, non multi-tenant)
- Implements: `IBranchService`, `IUserIntegrationService`, `IUserPermissionsCacheService`

### Module.Inventory

- Products, variants, brands, categories, colors, receptions, stock transfers
- Uses `IInvDbContext` (resolved from `AppDbContext` in `System.Infrastructure`)
- Implements: `IInventoryIntegrationService` (DeductStock, GetVariantsWithStock)

### Module.Sales

- Sales, sale items, cash register closures, cash register movements
- Uses `ISalesDbContext` (resolved from `AppDbContext` in `System.Infrastructure`)
- Cross-module transactions via `context.Database.BeginTransactionAsync()` (no longer uses `TransactionScope`)

---

## System.Api (Composition Root)

ASP.NET Core Web API host. References all projects and orchestrates wiring.

```
System.Api/
├── Program.cs                    # Composition Root
├── Controllers/                  # Presentation layer (Auth/, Branch/, Inventory/)
├── Middlewares/
│   ├── GlobalExceptionHandlerMiddleware.cs
│   └── TenantMiddleware.cs       # Resolves tenant from JWT
├── Filters/
│   ├── RequireFeatureFilter.cs   # Authorization filter by feature
│   └── ValidationFilter.cs       # ModelState → ProblemDetails
├── Result/
│   ├── ResultExtension.cs        # Result<T> → IActionResult
│   └── ValidationFilter.cs       # ModelState → ProblemDetails
└── Hubs/
    └── NotificationHub.cs        # SignalR
```

### Pipeline

```
Cors → GlobalExceptionHandler → HttpsRedirection → Authentication → Authorization → TenantMiddleware → Controllers
```

---

## Inter-Module Communication

Integration interfaces are defined in `Common/Contracts/` and implemented in the owning module's `Infrastructure` layer.

**Example:** Module.Sales needs to deduct stock:

1. `Common/Contracts/inventory/IInventoryIntegrationService` defines `DeductStock(StockDeductionDto)`
2. `Module.Inventory/Infrastructure/InventoryIntegrationService` implements the interface
3. `Module.Sales` injects `IInventoryIntegrationService` (from Common) — without knowing Module.Inventory
4. System.Api registers everything via DI

Dependency flow:

```
Module.Sales.Application.UseCases
    → IInventoryIntegrationService (in Common)
        → Module.Inventory.Infrastructure.InventoryIntegrationService (implementation)
```

### Cross-module transactions

Since Sales and Inventory share the same `AppDbContext`, transactions use native EF Core:

```csharp
await using var transaction = await context.Database.BeginTransactionAsync();
try
{
    inventoryService.DeductStock(deductions, branchId, userId, sale.Id);  // modifies entities
    context.Sales.Add(sale);
    await context.SaveChangesAsync();  // persists everything in a single call
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
}
```

No `TransactionScope` needed. The integration service (`DeductStock`) no longer calls `SaveChangesAsync` — the caller controls the transaction.

---

## Key patterns

| Pattern | Location |
|--------|-----------|
| **Clean Architecture** (3 layers) | Each module |
| **Result Pattern** | `Common/Utilities/Result.cs` — `Result<T>` without exceptions for business flows |
| **Use Case classes** | No MediatR, direct class injection |
| **Schema-based multi-tenancy** | `ITenantConnectionContext` + `IMustHaveTenant.HasQueryFilter` in `AppDbContext` |
| **Unified DbContext** | `System.Infrastructure.Persistence.AppDbContext` — Sales + Inventory together |
| **Separate Auth DbContext** | `AuthDbContext` (global, non multi-tenant) — fixed connection from config |
| **Native EF Core transactions** | `context.Database.BeginTransactionAsync()` — without `TransactionScope` |
| **Seeders via IDataSeeder** | Each module registers seeders, `DatabaseSeeder` aggregates and runs them in order |
| **Global Exception Handling** | `GlobalExceptionHandlerMiddleware` → RFC 7807 ProblemDetails |

---

## Projects and dependencies

```
System.Api (net9.0)
├── System.Infrastructure (net9.0)
│   ├── Common (net8.0)
│   ├── Module.Inventory (net9.0)
│   └── Module.Sales (net9.0)
├── Common (net8.0)
├── Module.Auth (net9.0)
├── Module.Inventory (net9.0)
└── Module.Sales (net9.0)

System.Infrastructure ──→ Common
System.Infrastructure ──→ Module.Inventory
System.Infrastructure ──→ Module.Sales
Module.Auth ────────────→ Common
Module.Inventory ───────→ Common
Module.Sales ───────────→ Common
```
