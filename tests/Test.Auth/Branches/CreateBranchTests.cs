using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.UseCases.Branches.CreateBranch;
using Module.Auth.Domain;
using Moq;

namespace Test.Auth.Branches;

public class CreateBranchTests
{
    [Fact]
    public async Task Execute_ShouldCreateBranch_WithTenantIdFromContext()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Basic",
            AllowedFeatureKeys = ["products", "recepciones", "pos"],
            DefaultRolesTemplate = [],
        };
        dbContext.Plans.Add(plan);

        dbContext.Features.AddRange(
            new Feature { Key = "products", Module = Module.Auth.Domain.Module.Inventory, DisplayName = "Products" },
            new Feature { Key = "recepciones", Module = Module.Auth.Domain.Module.Inventory, DisplayName = "Receptions" },
            new Feature { Key = "pos", Module = Module.Auth.Domain.Module.Sales, DisplayName = "POS" },
            new Feature { Key = "sales", Module = Module.Auth.Domain.Module.Sales, DisplayName = "Sales" });

        dbContext.Tenants.Add(new Tenant
        {
            Id = tenantId,
            DisplayName = "Test Tenant",
            PlanId = plan.Id,
            OwnerId = Guid.NewGuid()
        });
        await dbContext.SaveChangesAsync();

        var currentUser = Mock.Of<ICurrentUser>(u => u.TenantId == tenantId && u.UserId == Guid.NewGuid());
        var sut = new CreateBranch(dbContext, currentUser);

        var request = new CreateBranchRequest
        {
            Name = "Test Branch",
            Place = "Test Place",
            PhoneNumber = "123456789",
            BranchCode = "BR-001",
            Type = BranchType.Warehouse,
        };

        var result = await sut.Execute(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal("Test Branch", result.Value.Name);
        Assert.Equal(BranchType.Warehouse, result.Value.Type);
        Assert.Equal(["products", "recepciones"], result.Value.AllowedFeatureKeys);

        var savedBranch = await dbContext.Branches.FindAsync(result.Value.Id);
        Assert.NotNull(savedBranch);
        Assert.Equal(tenantId, savedBranch.TenantId);
    }

    [Fact]
    public async Task Execute_ShouldReturnPlanNotFound_WhenTenantHasNoPlan()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var currentUser = Mock.Of<ICurrentUser>(u => u.TenantId == tenantId);
        var sut = new CreateBranch(dbContext, currentUser);

        var result = await sut.Execute(new CreateBranchRequest
        {
            Name = "Test Branch",
            Place = "Test Place",
            PhoneNumber = "123456789",
            BranchCode = "BR-001",
            Type = BranchType.PointOfSale,
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(CreateBranchErrors.PlanNotFound, result.Error);
    }
}