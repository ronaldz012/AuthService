using Common.Utilities;
using Microsoft.Extensions.Configuration;
using Module.Auth.Application.Abstraction;
using Npgsql;

namespace Module.Auth.Infrastructure.Databases;

public class DbConnectionTester(IConfiguration configuration) : IDbConnectionTester
{
    public async Task<Result<bool>> TestConnectionAsync(string databaseName, string? schema = null)
    {
        var baseConnectionString = configuration.GetConnectionString("TenantConnection");
        if (string.IsNullOrEmpty(baseConnectionString))
            return DbConnectionTesterErrors.ConnectionStringNotFound;

        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = databaseName };
        if (!string.IsNullOrEmpty(schema)) builder.SearchPath = schema;

        try
        {
            await using var connection = new NpgsqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
    
            await using var command = new NpgsqlCommand($"SELECT current_schema();", connection);
            string? actualSchema = await command.ExecuteScalarAsync() as string;

            if (string.IsNullOrEmpty(actualSchema))
                return DbConnectionTesterErrors.SchemaNotFound;

            return true;
        }
        catch
        {
            return DbConnectionTesterErrors.ConnectionFailed;
        }
    }
}