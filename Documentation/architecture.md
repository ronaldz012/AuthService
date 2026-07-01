# DriveCoreSystem — Arquitectura

## Visión General

Monolito modular con Clean Architecture pragmática en .NET 8/9. Un solo deployment (System.Api) con bounded contexts separados en módulos independientes.

```
┌─────────────────────────────────────────────────────┐
│  System.Api (Composition Root / Host)               │
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
| `Contracts/` | Interfaces de comunicación entre módulos (`IInventoryIntegrationService`, `IBranchService`, `IUserIntegrationService`, `ITenantContext`) |
| `Contracts/Seeder/` | `IDataSeeder` + `DatabaseSeeder` (agregador) |
| `Domain/` | Interfaces base: `IMustHaveTenant`, `ISoftDelete`, `ICreatedAt`, `IUpdatedAt`, `ICreatedBy`, `IUpdatedBy` |
| `Utilities/` | `Result<T>` (Railway Oriented Programming), paginación, filtros dinámicos sobre `IQueryable` |
| `Services/` | `IEmailService`, configuraciones (`TokenSettings`, `SmtpSettings`, `TennantOptions`) |

---

## Módulos

Cada módulo es un proyecto independiente con 3 capas. **Nunca se referencian entre sí**, solo a Common.

### Estructura interna

```
Module.X/
├── Domain/                 # Entidades y enums, sin dependencias
├── Application/
│   ├── Abstraction/        # Interfaces de DbContext y servicios externos
│   └── UseCases/           # Casos de uso (un clase por operación)
└── Infrastructure/
    ├── Persistence/        # EF Core DbContext + migraciones
    ├── Seeder/             # Implementaciones de IDataSeeder
    └── Services/           # Implementaciones de contratos de Common
```

### Reglas de dependencia

- **Domain** — cero dependencias externas
- **Application** → depende de `Domain` + `Common.Contracts`
- **Infrastructure** → depende de `Application` + `Common`
- Módulos **no se referencian entre sí**

### Módulo.Auth

- Usuarios, roles, autenticación (JWT + Google OAuth), tenants, sucursales, features/permissions
- DbContext propio: `AuthDbContext`
- Implementa: `IBranchService`, `IUserIntegrationService`, `IUserPermissionsCacheService`

### Módulo.Inventory

- Productos, variantes, marcas, categorías, colores, recepciones, transferencias de stock
- DbContext propio: `InvDbContext`
- Implementa: `IInventoryIntegrationService` (DeductStock, GetVariantsWithStock)

### Módulo.Sales

- Ventas, items de venta, cierres de caja, movimientos de caja
- DbContext propio: `SalesDbContext`

---

## System.Api (Composition Root)

Host ASP.NET Core Web API. Referencia a todos los proyectos y orquesta el wiring.

```
System.Api/
├── Program.cs                    # Composition Root
├── Controllers/                  # Capa de presentación (Auth/, Branch/, Inventory/)
├── Middlewares/
│   ├── GlobalExceptionHandlerMiddleware.cs
│   └── TennantMiddleware.cs      # Resuelve tenant desde JWT
├── Attributes/
│   └── RequireFeatureAtribute.cs # [RequireFeature("module","permission")]
├── Filters/
│   ├── ApiKeyAttribute.cs
│   └── RequireFeatureFilter.cs   # Authorization filter
├── Result/
│   ├── ResultExtension.cs        # Result<T> → IActionResult
│   └── ValidationFilter.cs       # ModelState → ProblemDetails
└── Data/
    ├── DesignTimeDbContextFactory.cs
    └── Factory.cs                # Fábricas para migraciones
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

---

## Patrones clave

| Patrón | Ubicación |
|--------|-----------|
| **Clean Architecture** (3 capas) | Cada módulo |
| **Result Pattern** | `Common/Utilities/Result.cs` — `Result<T>` sin excepciones para flujos de negocio |
| **Use Case classes** | Sin MediatR, inyección directa de clases |
| **Multi-tenencia por esquema** | `ITenantContext` + `IMustHaveTenant.HasQueryFilter` en cada DbContext |
| **3 DbContexts separados** | Auth, Inventory, Sales — migraciones independientes |
| **Seeders vía IDataSeeder** | Cada módulo registra seeders, `DatabaseSeeder` los agrega y ejecuta en orden |
| **Global Exception Handling** | `GlobalExceptionHandlerMiddleware` → RFC 7807 ProblemDetails |

---

## Proyectos y dependencias

```
System.Api (net9.0)
├── Common (net8.0)
├── Module.Auth (net9.0)
├── Module.Inventory (net9.0)
└── Module.Sales (net9.0)

Module.Auth ────→ Common
Module.Inventory ─→ Common
Module.Sales ────→ Common
```
