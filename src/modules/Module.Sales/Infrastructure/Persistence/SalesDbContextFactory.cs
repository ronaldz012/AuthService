using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Module.Sales.Infrastructure.Persistence;

public class SalesDbContextFactory : IDesignTimeDbContextFactory<SalesDbContext>
{
    public SalesDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("TenantConnection")!;

        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql(connectionString, x =>
                x.MigrationsHistoryTable("__EFMigrationsHistory_sales"))
            .Options;

        return new SalesDbContext(options, new DesignTimeTenantConnectionContext());
    }
}
