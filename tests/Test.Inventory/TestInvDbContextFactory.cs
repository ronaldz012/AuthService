using System.Data.Common;
using System.Transactions;
using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Module.Inventory.Infrastructure.Persistence;

namespace Test.Inventory;

public static class TestInvDbContextFactory
{
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static InvDbContext Create(Guid? tenantId = null)
    {
        return Create(CreateTenantContext(tenantId));
    }

    public static InvDbContext Create(ITenantConnectionContext tenantConnectionContext, string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<InvDbContext>()
            .UseInMemoryDatabase(dbName ?? $"InvTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new InvDbContext(options, tenantConnectionContext);
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
    public Task EnsureOpenAsync() => Task.CompletedTask;
    public Task<TransactionScope> BeginTransactionScopeAsync() =>
        Task.FromResult(new TransactionScope(TransactionScopeOption.Suppress));
}
