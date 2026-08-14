using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.Contracts.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Infrastructure.Services;
using System.Infrastructure.Persistence;

namespace Test.Inventory;

public class DefaultCatalogProvisionerTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid UserId = Guid.NewGuid();

    private ServiceProvider BuildProvider(string dbName)
    {
        var tenantCtx = new TenantConnectionContext();

        var resolverMock = new Mock<ITenantDatabaseResolver>();
        resolverMock
            .Setup(r => r.GetTenantDatabaseInfo(TenantId))
            .ReturnsAsync(new TenantDatabaseInfoDto
            {
                TenantId = TenantId,
                Schema = "test_schema",
                DatabaseName = "test_database"
            });

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var services = new ServiceCollection();
        services.AddScoped<ITenantConnectionContext>(_ => tenantCtx);
        services.AddScoped<ITenantDatabaseResolver>(_ => resolverMock.Object);
        services.AddScoped(_ => new AppDbContext(options, tenantCtx));
        services.AddScoped<IInvDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IDefaultCatalogProvisioner, DefaultCatalogProvisioner>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SeedAsync_ShouldSeedCatalog_WhenTenantHasNone()
    {
        using var provider = BuildProvider($"cat_{Guid.NewGuid()}");

        using (var scope = provider.CreateScope())
        {
            var provisioner = scope.ServiceProvider.GetRequiredService<IDefaultCatalogProvisioner>();
            await provisioner.SeedAsync(TenantId, UserId, "System", DefaultCatalogTemplates.Basic);
        }

        using (var scope = provider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<IInvDbContext>();
            Assert.True(await ctx.Categories.AnyAsync());
            Assert.True(await ctx.Colors.AnyAsync());
            Assert.True(await ctx.Sizes.AnyAsync());
            Assert.Empty(await ctx.Brands.ToListAsync());
        }
    }

    [Fact]
    public async Task SeedAsync_ShouldBeIdempotent()
    {
        using var provider = BuildProvider($"cat_{Guid.NewGuid()}");

        using (var scope = provider.CreateScope())
        {
            var provisioner = scope.ServiceProvider.GetRequiredService<IDefaultCatalogProvisioner>();
            await provisioner.SeedAsync(TenantId, UserId, "System", DefaultCatalogTemplates.Basic);
            await provisioner.SeedAsync(TenantId, UserId, "System", DefaultCatalogTemplates.Basic);
        }

        using (var scope = provider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<IInvDbContext>();
            var count = await ctx.Categories.CountAsync();
            Assert.True(count > 0);
            Assert.Equal(count, await ctx.Categories.CountAsync());
        }
    }
}