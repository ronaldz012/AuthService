using System.Data.Common;
using System.Transactions;

namespace Common.Contracts.authentication;

public interface ITenantConnectionContext
{
    string? Schema { get; set; }
    Guid? TenantId { get; set; }
    string? DatabaseName { get; set; }
    DbConnection Connection { get; }
    Task EnsureOpenAsync();
    Task<TransactionScope> BeginTransactionScopeAsync();
}

public class DesignTimeTenantConnectionContext : ITenantConnectionContext
{
    public string? Schema { get; set; } = "base";
    public Guid? TenantId { get; set; } = new Guid();
    public string? DatabaseName { get; set; } = "base";
    public DbConnection Connection => throw new NotSupportedException("Design-time context does not support connection resolution.");
    public Task EnsureOpenAsync() => throw new NotSupportedException("Design-time context does not support connection resolution.");
    public Task<TransactionScope> BeginTransactionScopeAsync() => throw new NotSupportedException("Design-time context does not support connection resolution.");
}
