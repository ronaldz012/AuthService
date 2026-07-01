# Test Rules — Test.Auth

## Factory
- Use `TestAuthDbContextFactory` to create `AuthDbContext`.
- `Create(ITenantContext, string? dbName)` receives a shared TenantContext and optional dbName for multi-tenant tests.
- `CreateTenantContext(Guid? tenantId)` creates a `TenantContext` with test `Schema` and `DatabaseName`.

## Shared TenantContext
- **Always** share the same `TenantContext` instance between the `DbContext` and the SUT (use case).
- The SUT may mutate `tenantContext.TenantId` and the DbContext's `SaveChangesAsync` will read the updated value.

## SaveChangesAsync
- `AuthDbContext.SaveChangesAsync` auto-assigns `TenantId` to every `IMustHaveTenant` entity in `Added` state.
- On `Modified`, it prevents changes to `TenantId`.

## Multi-tenant isolation
- Use the same `dbName` to share the in-memory database across multiple `DbContext` instances.
- Each `DbContext` is created with its own `TenantContext` (different filter).
- Verify each tenant only sees its own data.
- At the end, use `.IgnoreQueryFilters()` to confirm all data lives in the same DB.

## Common assertions
- `result.IsSuccess` / `result.Error.Code` for use case results.
- `entity.TenantId == tenantId` to verify multi-tenant assignment.
- `savedRole.RoleFeaturePermissions.All(p => p.TenantId == tenantId)` for child entities.
