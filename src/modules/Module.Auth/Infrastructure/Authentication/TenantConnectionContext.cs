using System.Data.Common;
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
