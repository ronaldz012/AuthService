using System.Data.Common;
using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Module.Sales.Infrastructure.Persistence;

namespace Test.Sales;

public static class TestSalesDbContextFactory
{
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static SalesDbContext Create(Guid? tenantId = null)
    {
        return Create(CreateTenantContext(tenantId));
    }

    public static SalesDbContext Create(ITenantConnectionContext tenant, string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseInMemoryDatabase(dbName ?? $"SalesTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new SalesDbContext(options, tenant);
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

public class TenantConnectionContext : ITenantConnectionContext
{
    public string? Schema { get; set; }
    public Guid? TenantId { get; set; }
    public string? DatabaseName { get; set; }
    public DbConnection Connection => throw new NotSupportedException("InMemory tests do not support Connection.");
}
