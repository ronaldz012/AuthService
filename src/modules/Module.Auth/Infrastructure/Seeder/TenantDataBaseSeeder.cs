using Common.Contracts.Seeder;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Seeder;

public class TenantDataBaseSeeder(IAuthDbContext context)
    : IDataSeeder
{
    public int Order => 1;
    public async Task SeedAsync()
    {
        if(context.TenantDatabases.Any()) return;
        context.TenantDatabases.Add(new TenantDataBase
        {
            Name = "erp_db",
            Description = "this tenant is the first Tenant Database",
            Schema = "tenant_db",
        });
        await context.SaveChangesAsync();
    }
}