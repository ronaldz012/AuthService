using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Module.Auth.Infrastructure.Authentication;
using Module.Auth.Infrastructure.Persistence;

namespace Test.Auth;

public static class TestAuthDbContextFactory
{
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static AuthDbContext Create(Guid? tenantId = null)
    {
        return Create(CreateTenantContext(tenantId));
    }

    public static AuthDbContext Create(ITenantConnectionContext tenantConnectionContext, string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(dbName ?? $"AuthTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AuthDbContext(options, tenantConnectionContext);
    }

    public static TenantConnectionContext CreateTenantContext(Guid? tenantId = null)
    {
        return new TenantConnectionContext
        {
            TenantId = tenantId ?? DefaultTenantId,
            Schema = "test_schema",
            DatabaseName = "test_database",
        };
    }
}
