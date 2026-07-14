# Test Rules — Test.Sales

## Factory
- Use `TestSalesDbContextFactory` to create `SalesDbContext`.
- `Create(ITenantConnectionContext, string? dbName)` to share tenant context across SUT and context.

## Shared TenantContext
- Share the same `TenantConnectionContext` between the `DbContext` and the SUT (use case constructor).

## InMemory limitations
- `TransactionScope` is ignored (suppress `InMemoryEventId.TransactionIgnoredWarning`).
- `ITenantConnectionContext.Connection` is not available — mock `DbConnection` with `State = Open`.
- For integration tests requiring cross-module transactions, use PostgreSQL via Testcontainers.

## Mocking
- Mock `IInventoryIntegrationService` — it's an external module dependency.
- Mock `ICurrentUser` for branch and user identity.
- Mock `ILogger<T>`.

## Common assertions
- `result.IsSuccess` / `result.Error` for use case results.
- `entity.TenantId == tenantId` to verify multi-tenant assignment.
