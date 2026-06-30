using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Tenant.Create;
using Module.Auth.Domain;
using Module.Auth.Infrastructure.Persistence;

namespace Test.Auth;

public class CreateTenantTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenDatabaseNotFound()
    {
        using var dbContext = CreateDbContext();
        var sut = CreateSut(dbContext);

        var request = new CreateTenantRequest(
            "Test Tenant", "owner@test.com", Guid.NewGuid(),
            "owner", "Main Branch", "Some Place", "123456789", Guid.NewGuid());

        var result = await sut.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenDisplayNameAlreadyExists()
    {
        using var dbContext = CreateDbContext();

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

        var sut = CreateSut(dbContext);

        var request = new CreateTenantRequest(
            "Existing Tenant", "owner@test.com", databaseId,
            "owner", "Main Branch", "Some Place", "123456789", planId);

        var result = await sut.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("VALIDATION_ERROR", result.Error.Code);
        Assert.Contains("already exists", result.Error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenPlanNotFound()
    {
        using var dbContext = CreateDbContext();

        var databaseId = Guid.NewGuid();
        dbContext.TenantDatabases.Add(new TenantDataBase { Id = databaseId, Name = "TestDB", Description = "Test database", Schema = "test" });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext);

        var request = new CreateTenantRequest(
            "New Tenant", "owner@test.com", databaseId,
            "owner", "Main Branch", "Some Place", "123456789", Guid.NewGuid());

        var result = await sut.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenAllInputsAreValid()
    {
        using var dbContext = CreateDbContext();

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

        var sut = CreateSut(dbContext);

        var request = new CreateTenantRequest(
            "New Tenant", "owner@test.com", databaseId,
            "owner", "Main Branch", "Some Place", "123456789", planId);

        var result = await sut.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

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

        var savedCode = await dbContext.EmailVerificationCodes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ev => ev.TenantId == savedTenant.Id);
        Assert.NotNull(savedCode);
        Assert.Equal(savedTenant.OwnerId, savedCode.UserId);
        Assert.Equal(result.Value, savedCode.Code);
    }

    private static AuthDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase($"AuthTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AuthDbContext(options, new TenantContext());
    }

    private static CreateTenant CreateSut(IAuthDbContext dbContext)
    {
        return new CreateTenant(dbContext);
    }
}
