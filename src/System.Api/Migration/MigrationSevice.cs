using Auth.Data.Persistence;
using Branches.module.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using sales.Module.Data;
using Common.Data;
using Inventory.Data;

namespace System.Api.Migration;

public class MigrationService(
    IServiceScopeFactory scopeFactory, 
    IConfiguration configuration)
{
    public async Task MigrateAuthTenantAsync(string schema)
    {
        var tenantConnection = GetTenantConnection(schema);
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(tenantConnection, x =>
                x.MigrationsHistoryTable("__EFMigrationsHistory_auth", schema)).Options;

        await using var dbContext = new AuthDbContext(options, new DesignTimeTenantContext());
        await dbContext.Database.MigrateAsync();
    }

    public async Task MigrateBranchTenantAsync(string schema)
    {
        var tenantConnection = GetTenantConnection(schema);
        var options = new DbContextOptionsBuilder<BranchDbContext>()
            .UseNpgsql(tenantConnection, x =>
                x.MigrationsHistoryTable("__EFMigrationsHistory_branches", schema)).Options;

        await using var dbContext = new BranchDbContext(options, new DesignTimeTenantContext());
        await dbContext.Database.MigrateAsync();
    }

    public async Task MigrateInvTenantAsync(string schema)
    {
        var tenantConnection = GetTenantConnection(schema);
        var options = new DbContextOptionsBuilder<InvDbContext>()
            .UseNpgsql(tenantConnection, x =>
                x.MigrationsHistoryTable("__EFMigrationsHistory_inventory", schema)).Options;

        await using var dbContext = new InvDbContext(options, new DesignTimeTenantContext());
        await dbContext.Database.MigrateAsync();
    }

    public async Task MigrateSalesTenantAsync(string schema)
    {
        var tenantConnection = GetTenantConnection(schema);
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql(tenantConnection, x =>
                x.MigrationsHistoryTable("__EFMigrationsHistory_sales", schema)).Options;

        await using var dbContext = new SalesDbContext(options, new DesignTimeTenantContext());
        await dbContext.Database.MigrateAsync();
    }

    private string GetTenantConnection(string schema)
    {
        var baseConnection = configuration.GetConnectionString("DefaultConnection")!;
        
        // Crear el schema si no existe
        using (var conn = new NpgsqlConnection(baseConnection))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"CREATE SCHEMA IF NOT EXISTS \"{schema}\"";
            cmd.ExecuteNonQuery();
        }

        return new NpgsqlConnectionStringBuilder(baseConnection)
        {
            SearchPath = schema
        }.ConnectionString;
    }
}