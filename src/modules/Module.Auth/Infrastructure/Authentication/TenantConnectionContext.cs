using System.Data;
using System.Data.Common;
using System.Transactions;
using Common.Contracts.authentication;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Module.Auth.Infrastructure.Authentication;

public class TenantConnectionContext(IConfiguration configuration) : ITenantConnectionContext
{
    private DbConnection? _connection;

    public string? Schema { get; set; }
    public Guid? TenantId { get; set; }
    public string? DatabaseName { get; set; }

    public DbConnection Connection => _connection ??= BuildConnection();

    public async Task EnsureOpenAsync()
    {
        if (Connection.State != ConnectionState.Open)
            await Connection.OpenAsync();
    }

    public async Task<TransactionScope> BeginTransactionScopeAsync()
    {
        var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(30)
            },
            TransactionScopeAsyncFlowOption.Enabled);

        await EnsureOpenAsync();
        return scope;
    }

    private DbConnection BuildConnection()
    {
        if (string.IsNullOrEmpty(DatabaseName) || string.IsNullOrEmpty(Schema))
            throw new InvalidOperationException("Tenant DatabaseName or Schema is not set.");

        var baseConn = configuration.GetConnectionString("TenantConnection")!;
        var builder = new NpgsqlConnectionStringBuilder(baseConn)
        {
            Database = DatabaseName,
            SearchPath = Schema,
        };
        return new NpgsqlConnection(builder.ConnectionString);
    }
}
