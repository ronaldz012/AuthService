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
            return new Error("NOT_FOUND", "Connection string 'TenantConnection' not found");

        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = databaseName };
        if (!string.IsNullOrEmpty(schema)) builder.SearchPath = schema;

        try
        {
            await using var connection = new NpgsqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
    
            await using var command = new NpgsqlCommand($"SELECT current_schema();", connection);
            string? actualSchema = await command.ExecuteScalarAsync() as string;

            if (string.IsNullOrEmpty(actualSchema))
                return new Error("SCHEMA_NOT_FOUND", $"El esquema '{schema}' no existe en la base de datos.");

            return true;
        }
        catch (Exception ex)
        {
            return new Error("CONNECTION_FAILED", ex.Message);
        }
    }
}