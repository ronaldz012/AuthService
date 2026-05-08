using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Common.Data;

namespace System.Api.Data;

public abstract class DesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly string _migrationsTable;

    protected DesignTimeDbContextFactory(string migrationsTable)
    {
        _migrationsTable = migrationsTable;
    }

    protected abstract TContext CreateInstance(DbContextOptions<TContext> options, ITenantContext tenant);

    public TContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        var options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(connectionString, x =>
                x.MigrationsHistoryTable(_migrationsTable))
            .Options;

        return CreateInstance(options, new DesignTimeTenantContext());
    }
}