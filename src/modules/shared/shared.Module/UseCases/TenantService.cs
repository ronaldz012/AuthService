using Auth.Contracts.Dtos.Users;
using Auth.Contracts.Interfaces;
using Auth.Data;
using Auth.Data.Entities;
using Branches.Contracts;
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
    IUserIntegrationService userIntegrationService,
    IBranchService branchService, 
    ITenantContext tenantContext,
    IConfiguration configuration) // Usado para "saltar" al nuevo esquema
{
    public async Task<Result<Guid>> CreateTenantAsync(CreateTenantDto dto)
    {
        // 1. Validación de negocio
        if (await sharedContext.Tenants.AnyAsync(t => t.DisplayName == dto.DisplayName))
            return new Error("CONFLICT", "El nombre ya está registrado.");

        // 2. Validación de infraestructura
        var infraValidation = await ValidateDatabaseAndSchema(dto.DatabaseName, dto.Schema);
        if (!infraValidation.IsSuccess) return infraValidation.Error!;

        // 3. Persistir tenant en SharedDb
        var newTenant = new Tenant
        {
            DisplayName  = dto.DisplayName,
            Schema       = dto.Schema,
            DatabaseName = dto.DatabaseName,
        };

        sharedContext.Tenants.Add(newTenant);

        using var sharedTransaction = await sharedContext.Database.BeginTransactionAsync();
        try
        {
            await sharedContext.SaveChangesAsync();

            // 4. Saltar al schema del nuevo tenant para crear el admin
            tenantContext.TenantId    = newTenant.Id;
            tenantContext.Schema      = newTenant.Schema;
            tenantContext.DatabaseName = newTenant.DatabaseName;

            // 5. Crear usuario admin en AuthDbContext (schema del tenant)
            var adminResult = await userIntegrationService.CreateTenantAdminAsync( dto.AdminEmail, dto.AdminPassword);

            if (!adminResult.IsSuccess)
            {
                // Compensación — el tenant no queda huérfano sin admin
                await sharedTransaction.RollbackAsync();
                return adminResult.Error;
            }
            var branchResult = await branchService.CreateBranch(new Branches.Contracts.Dtos.CreateBranchDto
            {
                Name = dto.BranchName,
                Place = dto.BranchPlace,
                PhoneNumber = dto.BranchPhoneNumber,
                BranchCode = dto.BranchCode
            });
            if(!branchResult.IsSuccess)
            {
                await sharedTransaction.RollbackAsync();
                return branchResult.Error;
            }

            await sharedTransaction.CommitAsync();
            return newTenant.Id;
        }
        catch (Exception ex)
        {
            await sharedTransaction.RollbackAsync();
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