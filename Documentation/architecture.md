# DriveCoreSystem — Arquitectura

## Visión General

Monolito modular con Clean Architecture pragmática en .NET 8/9. Un solo deployment (System.Api) con bounded contexts separados en módulos independientes.

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

Librería transversal sin dependencias hacia los módulos.

| Carpeta | Contenido |
|---------|-----------|
| `Contracts/` | Interfaces de comunicación entre módulos (`IInventoryIntegrationService`, `IBranchService`, `IUserIntegrationService`, `ITenantConnectionContext`) |
| `Contracts/Seeder/` | `IDataSeeder` + `DatabaseSeeder` (agregador) |
| `Domain/` | Interfaces base: `IMustHaveTenant`, `ISoftDelete`, `ICreatedAt`, `IUpdatedAt`, `ICreatedBy`, `IUpdatedBy` |
| `Utilities/` | `Result<T>` (Railway Oriented Programming), paginación, filtros dinámicos sobre `IQueryable` |
| `Services/` | `IEmailService`, configuraciones (`TokenSettings`, `SmtpSettings`, `TennantOptions`) |

---

## System.Infrastructure (DbContext Unificado)

Proyecto compartido que contiene el **único DbContext** (AppDbContext) para Sales + Inventory. Los módulos Auth tiene su propio DbContext (global, no multi-tenant).

### Responsabilidades

- `Persistence/AppDbContext` — implementa `ISalesDbContext` e `IInvDbContext`
- `Persistence/AppDbContextFactory` — fábrica design-time para migraciones
- `SystemInfrastructureDependencyInjection` — registra `AppDbContext` y forwarding a ambas interfaces

```
AppDbContext : ISalesDbContext, IInvDbContext
├── Sales: Sale, SaleItem, CashRegisterClosure, CashRegisterMovement
└── Inventory: Product, ProductVariant, BranchInventory, Category, Provider,
               Brand, Color, StockReception, StockReceptionItem,
               StockMovement, StockTransfer, StockTransferItem
```

### Configuración

Un solo `AddDbContext<AppDbContext>` con `ITenantConnectionContext.Connection` (misma conexión física Npgsql para toda la request). Una sola tabla `__EFMigrationsHistory`.

```csharp
services.AddDbContext<AppDbContext>((sp, options) =>
{
    var tenant = sp.GetRequiredService<ITenantConnectionContext>();
    options.UseNpgsql(tenant.Connection,
        x => x.MigrationsHistoryTable("__EFMigrationsHistory"));
});

services.AddScoped<ISalesDbContext>(sp => sp.GetRequiredService<AppDbContext>());
services.AddScoped<IInvDbContext>(sp => sp.GetRequiredService<AppDbContext>());
```

---

## Módulos

Cada módulo es un proyecto independiente con 3 capas. **Nunca se referencian entre sí**, solo a Common y (según el caso) a `System.Infrastructure`.

### Estructura interna

```
Module.X/
├── Domain/                 # Entidades y enums, sin dependencias
├── Application/
│   ├── Abstraction/        # Interfaces de DbContext y servicios externos
│   └── UseCases/           # Casos de uso (un clase por operación)
└── Infrastructure/
    ├── Seeder/             # Implementaciones de IDataSeeder
    └── Services/           # Implementaciones de contratos de Common
```

> Nota: Persistence ya no existe en los módulos. El DbContext unificado está en `System.Infrastructure`.

### Reglas de dependencia

- **Domain** — cero dependencias externas
- **Application** → depende de `Domain` + `Common.Contracts`
- **Infrastructure** → depende de `Application` + `Common`
- Módulos **no se referencian entre sí**

### Módulo.Auth

- Usuarios, roles, autenticación (JWT + Google OAuth), tenants, sucursales, features/permissions
- DbContext propio: `AuthDbContext` (global, no multi-tenant)
- Implementa: `IBranchService`, `IUserIntegrationService`, `IUserPermissionsCacheService`

### Módulo.Inventory

- Productos, variantes, marcas, categorías, colores, recepciones, transferencias de stock
- Usa `IInvDbContext` (resuelto desde `AppDbContext` en `System.Infrastructure`)
- Implementa: `IInventoryIntegrationService` (DeductStock, GetVariantsWithStock)

### Módulo.Sales

- Ventas, items de venta, cierres de caja, movimientos de caja
- Usa `ISalesDbContext` (resuelto desde `AppDbContext` en `System.Infrastructure`)
- Cross-module transactions vía `context.Database.BeginTransactionAsync()` (ya no usa `TransactionScope`)

---

## System.Api (Composition Root)

Host ASP.NET Core Web API. Referencia a todos los proyectos y orquesta el wiring.

```
System.Api/
├── Program.cs                    # Composition Root
├── Controllers/                  # Capa de presentación (Auth/, Branch/, Inventory/)
├── Middlewares/
│   ├── GlobalExceptionHandlerMiddleware.cs
│   └── TenantMiddleware.cs       # Resuelve tenant desde JWT
├── Filters/
│   ├── RequireFeatureFilter.cs   # Authorization filter por feature
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

## Comunicación entre módulos

Las integraciones se definen como interfaces en `Common/Contracts/` y se implementan en la capa `Infrastructure` del módulo propietario.

**Ejemplo:** Module.Sales necesita deducir stock:

1. `Common/Contracts/inventory/IInventoryIntegrationService` define `DeductStock(StockDeductionDto)`
2. `Module.Inventory/Infrastructure/InventoryIntegrationService` implementa la interfaz
3. `Module.Sales` inyecta `IInventoryIntegrationService` (desde Common) — sin conocer Module.Inventory
4. System.Api registra todo vía DI

Flujo de dependencias:

```
Module.Sales.Application.UseCases
    → IInventoryIntegrationService (en Common)
        → Module.Inventory.Infrastructure.InventoryIntegrationService (implementación)
```

### Transacciones cross-module

Como Sales e Inventory comparten el mismo `AppDbContext`, las transacciones se manejan con EF Core nativo:

```csharp
await using var transaction = await context.Database.BeginTransactionAsync();
try
{
    inventoryService.DeductStock(deductions, branchId, userId, sale.Id);  // modifica entidades
    context.Sales.Add(sale);
    await context.SaveChangesAsync();  // persiste todo en una sola llamada
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
}
```

No se necesita `TransactionScope`. La integración service (`DeductStock`) ya no llama `SaveChangesAsync` — el caller controla la transacción.

---

## Patrones clave

| Patrón | Ubicación |
|--------|-----------|
| **Clean Architecture** (3 capas) | Cada módulo |
| **Result Pattern** | `Common/Utilities/Result.cs` — `Result<T>` sin excepciones para flujos de negocio |
| **Use Case classes** | Sin MediatR, inyección directa de clases |
| **Multi-tenencia por esquema** | `ITenantConnectionContext` + `IMustHaveTenant.HasQueryFilter` en `AppDbContext` |
| **DbContext unificado** | `System.Infrastructure.Persistence.AppDbContext` — Sales + Inventory juntos |
| **Auth DbContext separado** | `AuthDbContext` (global, sin multi-tenant) — conexión fija desde config |
| **Transacciones nativas EF Core** | `context.Database.BeginTransactionAsync()` — sin `TransactionScope` |
| **Seeders vía IDataSeeder** | Cada módulo registra seeders, `DatabaseSeeder` los agrega y ejecuta en orden |
| **Global Exception Handling** | `GlobalExceptionHandlerMiddleware` → RFC 7807 ProblemDetails |

---

## Proyectos y dependencias

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
