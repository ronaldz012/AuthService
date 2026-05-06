using Microsoft.Extensions.DependencyInjection;
using Shared.Data;

namespace Shared.Services;

public interface IMigrationService
{
    Task MigrateTenantAsync(string schema);
}

public class MigrationService(IServiceScopeFactory scopeFactory) : IMigrationService
{
    public async Task MigrateTenantAsync(string schema)
    {
        using var scope = scopeFactory.CreateScope();
        
        // 1. Forzamos el esquema en el TenantContext del scope actual
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.Schema = schema;

        // 2. Obtenemos el DbContext (que leerá el esquema que acabamos de setear)
        var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();

        // 3. Ejecutamos la migración
        await dbContext.Database.MigrateAsync();
    }
}