using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.Contracts.inventory;
using Module.Inventory.Infrastructure.Seeder;
using Moq;

namespace Test.Inventory;

public class DefaultCatalogSeederTests
{
    [Fact]
    public async Task SeedAsync_ShouldCallProvisioner_ForEveryTenant()
    {
        var tenants = new List<TenantDatabaseInfoDto>
        {
            new() { TenantId = Guid.NewGuid(), Schema = "s1", DatabaseName = "d1", OwnerUserId = Guid.NewGuid() },
            new() { TenantId = Guid.NewGuid(), Schema = "s2", DatabaseName = "d2", OwnerUserId = Guid.NewGuid() },
        };

        var resolverMock = new Mock<ITenantDatabaseResolver>();
        resolverMock.Setup(r => r.GetAll()).ReturnsAsync(tenants);

        var provisionerMock = new Mock<IDefaultCatalogProvisioner>();

        var seeder = new DefaultCatalogSeeder(resolverMock.Object, provisionerMock.Object);
        await seeder.SeedAsync();

        provisionerMock.Verify(
            p => p.SeedAsync(
                tenants[0].TenantId,
                tenants[0].OwnerUserId,
                It.IsAny<string>(),
                DefaultCatalogTemplates.Basic),
            Times.Once);

        provisionerMock.Verify(
            p => p.SeedAsync(
                tenants[1].TenantId,
                tenants[1].OwnerUserId,
                It.IsAny<string>(),
                DefaultCatalogTemplates.Basic),
            Times.Once);
    }
}