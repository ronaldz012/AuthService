using Auth.Data;
using Auth.Data.Entities;
using Common.Data;
using Common.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using shared.Contracts.dtos;
using shared.Module.Data;
using shared.Module.Entities;

namespace shared.Module.UseCases;

public class TenantService(
    SharedDbContext sharedContext,
    AuthDbContext authContext,
    ITenantContext tenantContext,
    IConfiguration configuration) // Usado para "saltar" al nuevo esquema
{
    public async Task<Result<Guid>> CreateTenantAsync(CreateTenantDto dto)
    {
        // 1. Validaciones de Negocio (DisplayName único)
        if (await sharedContext.Tenants.AnyAsync(t => t.DisplayName == dto.DisplayName))
            return new Error("CONFLICT", "El nombre ya está registrado.");

        // 2. Validaciones de Infraestructura (DB y Schema)
        var infraValidation = await ValidateDatabaseAndSchema(dto.DatabaseName, dto.Schema);
        if (!infraValidation.IsSuccess) return infraValidation.Error!;

        // 3. Persistencia del Tenant
        var newTenant = new Tenant
        {
            DisplayName = dto.DisplayName,
            Schema = dto.Schema,
            DatabaseName = dto.DatabaseName,
        };

        sharedContext.Tenants.Add(newTenant);

        // 4. Creación del Usuario Admin (Cross-Context Transaction)
        using var transaction = await sharedContext.Database.BeginTransactionAsync();
        try 
        {
            await sharedContext.SaveChangesAsync();

            // Cambiamos el contexto para apuntar al nuevo "mundo"
            tenantContext.TenantId = newTenant.Id;
            tenantContext.Schema = newTenant.Schema;

            //await CreateDefaultAdmin(newTenant.Id, dto.AdminEmail, dto.AdminPassword);
        
            await transaction.CommitAsync();
            return newTenant.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new Error("INTERNAL_ERROR", ex.Message);
        }
    }

    private string BuildCustomConnectionString(string dbName)
    {
        var builder = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));
        builder.Database = dbName;
        return builder.ConnectionString;
    }



    private async Task<Result<bool>> ValidateDatabaseAndSchema(string? dbName, string schema)
    {
        var mainConnectionString = configuration.GetConnectionString("DefaultConnection")!;
        var builder = new NpgsqlConnectionStringBuilder(mainConnectionString);
    
        if (!string.IsNullOrEmpty(dbName))
        {
            var targetDb = dbName.ToLower();
            // Nos conectamos a la DB por defecto ('postgres') para consultar 'pg_database'
            builder.Database = "postgres"; 
        
            using var conn = new NpgsqlConnection(builder.ConnectionString);
            await conn.OpenAsync();
        
            using var cmd = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @db)", conn);
            cmd.Parameters.AddWithValue("db", targetDb);
        
            var dbExists = (bool)(await cmd.ExecuteScalarAsync() ?? false);
            if (!dbExists) return new Error("NOT_FOUND", $"La base de datos '{targetDb}' no existe en el servidor.");
        
            // Cambiamos el builder a la DB destino para el siguiente paso
            builder.Database = targetDb;
        }

        // 2. Validar existencia del Esquema
        try
        {
            using var conn = new NpgsqlConnection(builder.ConnectionString);
            await conn.OpenAsync();
        
            using var cmd = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = @sch)", conn);
            cmd.Parameters.AddWithValue("sch", schema.ToLower());
        
            var schemaExists = (bool)(await cmd.ExecuteScalarAsync() ?? false);
            if (!schemaExists) return new Error("NOT_FOUND", $"El esquema '{schema}' no existe en la base de datos.");
        }
        catch (NpgsqlException)
        {
            return new Error("CONNECTION_ERROR", "No se pudo establecer conexión con la base de datos destino.");
        }

        return true;
    }
}