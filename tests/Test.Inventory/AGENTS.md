# Test Rules — Test.Inventory

## Factory
- Use `TestInvDbContextFactory` to create `InvDbContext`.
- `Create(ITenantConnectionContext, string? dbName)` receives a shared tenant context and optional dbName for multi-tenant tests.
- `CreateTenantContext(Guid? tenantId)` creates a `TenantConnectionContext` with test `Schema` and `DatabaseName`.

## Shared TenantContext
- **Always** share the same `TenantConnectionContext` instance between the `DbContext` and the SUT (use case).
- The SUT may mutate `tenantContext.TenantId` and the DbContext's `SaveChangesAsync` will read the updated value.

## InMemory limitations
- Raw SQL (`ReserveBrandCounter`, `ReserveVariantCounter`) is not supported by InMemory — mock `IInvDbContext` for use cases that call these methods.
- For use cases that only query/save via EF, use the real `InvDbContext` with InMemory.

## Common assertions
- `result.IsSuccess` / `result.Error.Code` for use case results.
- `entity.TenantId == tenantId` to verify multi-tenant assignment.
