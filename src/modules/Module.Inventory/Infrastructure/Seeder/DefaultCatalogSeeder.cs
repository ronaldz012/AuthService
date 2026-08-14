using Common.Contracts.authentication;
using Common.Contracts.Seeder;
using Common.Contracts.inventory;

namespace Module.Inventory.Infrastructure.Seeder;

public class DefaultCatalogSeeder(
    ITenantDatabaseResolver tenantResolver,
    IDefaultCatalogProvisioner catalogProvisioner) : IDataSeeder
{
    public int Order => 6;

    public async Task SeedAsync()
    {
        var tenants = await tenantResolver.GetAll();

        foreach (var tenant in tenants)
        {
            await catalogProvisioner.SeedAsync(
                tenant.TenantId,
                tenant.OwnerUserId,
                "System",
                DefaultCatalogTemplates.Basic);
        }
    }
}