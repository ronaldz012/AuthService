# DriveCore System — Project Rules

This is a modular ERP built with .NET 9, EF Core, and PostgreSQL. Each module (Auth, Inventory, Sales) lives in `src/modules/Module.*`. The shared DbContext lives in `src/System.Infrastructure`.

## Architecture

Read `Documentation/architecture.md` for the full system overview (module structure, communication patterns, transaction flow).

## Rule Files

Detailed conventions are in `.agents/rules/`. Load them on demand based on the task:

| File | When to load |
|---|---|
| `.agents/rules/usecase-patterns.md` | Creating/modifying use cases in any module |
| `.agents/rules/controller-patterns.md` | Creating/modifying API controllers |
| `.agents/rules/cross-module-transactions.md` | Cross-module transactions or BeginTransactionAsync |
| `.agents/rules/domain-guidelines.md` | Writing domain entities or value objects |
| `.agents/rules/integrations-service.md` | Writing integration services between modules |

## Module-Specific Docs

| File | When to load |
|---|---|
| `src/modules/Module.Auth/AGENTS.md` | Working on Auth (roles, permissions, login flow) |

## Build & Test

```
dotnet build src/System.Api/System.Api.csproj
dotnet test tests/Test.Sales/
dotnet test tests/Test.Inventory/
dotnet test tests/Test.Auth/
```

## Database Migrations

Run all `dotnet ef` commands from `src/System.Api` (the design-time factories load `appsettings.json` from there).

### AppDbContext (Inventory + Sales — tenant schema)

```
# Create a migration
dotnet ef migrations add <MigrationName> \
  --project src/System.Infrastructure/System.Infrastructure.csproj \
  --startup-project src/System.Api/System.Api.csproj \
  --context AppDbContext

# Apply pending migrations
dotnet ef database update \
  --project src/System.Infrastructure/System.Infrastructure.csproj \
  --startup-project src/System.Api/System.Api.csproj \
  --context AppDbContext
```

### AuthDbContext (Auth — public schema, table `__EFMigrationsHistory_shared`)

```
# Create a migration
dotnet ef migrations add <MigrationName> \
  --project src/modules/Module.Auth/Module.Auth.csproj \
  --startup-project src/System.Api/System.Api.csproj \
  --context AuthDbContext

# Apply pending migrations
dotnet ef database update \
  --project src/modules/Module.Auth/Module.Auth.csproj \
  --startup-project src/System.Api/System.Api.csproj \
  --context AuthDbContext
```

> Both contexts point to the same database (`erp_db`). `AppDbContext` uses the `TenantConnection` string (schema `tenant_db`); `AuthDbContext` uses `DefaultConnection` (schema `public`).
