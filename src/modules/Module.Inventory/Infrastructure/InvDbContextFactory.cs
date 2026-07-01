using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Module.Inventory.Infrastructure.Persistence;

namespace Module.Inventory.Infrastructure;

public class InvDbContextFactory : IDesignTimeDbContextFactory<InvDbContext>
{
    public InvDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("TenantConnection")!;

        var options = new DbContextOptionsBuilder<InvDbContext>()
            .UseNpgsql(connectionString, x =>
                x.MigrationsHistoryTable("__EFMigrationsHistory_inventory"))
            .Options;

        return new InvDbContext(options, new DesignTimeTenantContext());
    }
}
