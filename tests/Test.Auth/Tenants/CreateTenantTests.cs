using Common.Contracts.authentication;
using Common.Contracts.inventory;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Tenant.Create;
using Module.Auth.Domain;
using Moq;

namespace Test.Auth.Tenants;

public class CreateTenantTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenDatabaseNotFound()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);
        var sut = CreateSut(dbContext, tenantContext);

        var request = new CreateTenantRequest(
            "Test Tenant", "owner@test.com", Guid.NewGuid(),
            "owner", "Main Branch", "Some Place", "123456789", Guid.NewGuid(),
            SendEmail: true);

        var result = await sut.ExecuteAsync(request);

        Assert.Equal(CreateTenantErrors.DatabaseNotFound, result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenDisplayNameAlreadyExists()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var databaseId = Guid.NewGuid();
        dbContext.TenantDatabases.Add(new TenantDataBase { Id = databaseId, Name = "TestDB", Description = "Test database", Schema = "test" });

        var planId = Guid.NewGuid();
        dbContext.Plans.Add(new Plan { Id = planId, Name = "Basic", DefaultRolesTemplate = [] });

        dbContext.Tenants.Add(new Tenant
        {
            Id = Guid.NewGuid(),
            DisplayName = "Existing Tenant",
            DataBaseId = databaseId,
            PlanId = planId,
            OwnerId = Guid.NewGuid()
        });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext, tenantContext);

        var request = new CreateTenantRequest(
            "Existing Tenant", "owner@test.com", databaseId,
            "owner", "Main Branch", "Some Place", "123456789", planId,
            SendEmail: true);

        var result = await sut.ExecuteAsync(request);

        Assert.Equal(CreateTenantErrors.TenantAlreadyExists, result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenPlanNotFound()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var databaseId = Guid.NewGuid();
        dbContext.TenantDatabases.Add(new TenantDataBase { Id = databaseId, Name = "TestDB", Description = "Test database", Schema = "test" });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext, tenantContext);

        var request = new CreateTenantRequest(
            "New Tenant", "owner@test.com", databaseId,
            "owner", "Main Branch", "Some Place", "123456789", Guid.NewGuid(),
            SendEmail: true);

        var result = await sut.ExecuteAsync(request);

        Assert.Equal(CreateTenantErrors.PlanNotFound, result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenAllInputsAreValid()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var databaseId = Guid.NewGuid();
        dbContext.TenantDatabases.Add(new TenantDataBase { Id = databaseId, Name = "TestDB", Description = "Test database", Schema = "test" });

        var planId = Guid.NewGuid();
        dbContext.Plans.Add(new Plan
        {
            Id = planId,
            Name = "Basic",
            DefaultRolesTemplate =
            [
                new DefaultRoleTemplate
                {
                    Name = "Admin",
                    Description = "Administrator role",
                    Permissions =
                    [
                        new DefaultRolePermissionTemplate
                        {
                            FeatureKey = "dashboard",
                            Actions = ["view", "edit"]
                        }
                    ]
                }
            ]
        });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext, tenantContext);

        var request = new CreateTenantRequest(
            "New Tenant", "owner@test.com", databaseId,
            "owner", "Main Branch", "Some Place", "123456789", planId,
            SendEmail: true);

        var result = await sut.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("New Tenant", result.Value.DisplayName);

        var savedTenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.DisplayName == "New Tenant");
        Assert.NotNull(savedTenant);

        var savedBranch = await dbContext.Branches
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.TenantId == savedTenant.Id);
        Assert.NotNull(savedBranch);
        Assert.Equal("Main Branch", savedBranch.Name);

        var savedRoles = await dbContext.Roles
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == savedTenant.Id)
            .ToListAsync();
        Assert.Single(savedRoles);
        Assert.Equal("Admin", savedRoles[0].Name);
    }

    private static CreateTenant CreateSut(IAuthDbContext dbContext, ITenantConnectionContext tenantConnectionContext)
    {
        var projectInfoOptions = Options.Create(new ProjectInfo
        {
            AppBranding = new AppBranding { FrontendDomain = "test.com" },
            EmailTemplateDefaults = new EmailTemplateDefaults()
        });

        var auth0Mock = new Mock<IAuth0ProvisioningService>();
        auth0Mock.Setup(x => x.CreateInvitationUserAsync(It.IsAny<string>()))
            .ReturnsAsync("auth0|test123");
        auth0Mock.Setup(x => x.CreatePasswordChangeTicketAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("https://test.com/ticket");

        return new CreateTenant(
            dbContext,
            tenantConnectionContext,
            auth0Mock.Object,
            Mock.Of<IDefaultCatalogProvisioner>(),
            projectInfoOptions,
            Mock.Of<ILogger<CreateTenant>>());
    }
}
