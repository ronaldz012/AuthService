using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.UseCases.Branches.GetBranches;
using Module.Auth.Domain;

namespace Test.Auth.Branches;

public class GetBranchesTests
{
    [Fact]
    public async Task Execute_ShouldReturn_OnlyOwnTenantBranches()
    {
        var dbName = $"GetBranches_{Guid.NewGuid()}";
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        var tenantContextA = TestAuthDbContextFactory.CreateTenantContext(tenantAId);
        using (var dbContextA = TestAuthDbContextFactory.Create(tenantContextA, dbName))
        {
            dbContextA.Branches.Add(new Branch
            {
                Id = Guid.NewGuid(),
                Name = "Branch A1",
                Place = "Place A1",
                PhoneNumber = "111",
                CreatedAt = DateTime.UtcNow,
            });
            dbContextA.Branches.Add(new Branch
            {
                Id = Guid.NewGuid(),
                Name = "Branch A2",
                Place = "Place A2",
                PhoneNumber = "112",
                CreatedAt = DateTime.UtcNow,
            });
            await dbContextA.SaveChangesAsync();
        } 

        var tenantContextB = TestAuthDbContextFactory.CreateTenantContext(tenantBId);
        using (var dbContextB = TestAuthDbContextFactory.Create(tenantContextB, dbName))
        {
            dbContextB.Branches.Add(new Branch
            {
                Id = Guid.NewGuid(),
                Name = "Branch B1",
                Place = "Place B1",
                PhoneNumber = "211",
                CreatedAt = DateTime.UtcNow,
            });
            await dbContextB.SaveChangesAsync();
        }

        // Act & Assert para Tenant A
        using (var dbReadA = TestAuthDbContextFactory.Create(tenantContextA, dbName))
        {
            var sutA = new GetBranches(dbReadA);
            var resultA = await sutA.Execute();

            Assert.True(resultA.IsSuccess);
            Assert.Equal(2, resultA.Value.Count);
            Assert.Contains(resultA.Value, b => b.Name == "Branch A1");
            Assert.Contains(resultA.Value, b => b.Name == "Branch A2");
        }

        // Act & Assert para Tenant B
        using (var dbReadB = TestAuthDbContextFactory.Create(tenantContextB, dbName))
        {
            var sutB = new GetBranches(dbReadB);
            var resultB = await sutB.Execute();

            Assert.True(resultB.IsSuccess);
            Assert.Single(resultB.Value);
            Assert.Equal("Branch B1", resultB.Value[0].Name);
        }

        // Verify all 3 branches exist in the same database (IgnoreQueryFilters)
        using (var dbAll = TestAuthDbContextFactory.Create(tenantContextA, dbName))
        {
            var allBranches = await dbAll.Branches
                .IgnoreQueryFilters()
                .ToListAsync();
            Assert.Equal(3, allBranches.Count);
            Assert.Contains(allBranches, b => b.Name == "Branch A1");
            Assert.Contains(allBranches, b => b.Name == "Branch A2");
            Assert.Contains(allBranches, b => b.Name == "Branch B1");
        }
    }

    [Fact]
    public async Task Execute_ShouldReturnEmpty_WhenNoBranchesExist()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var sut = new GetBranches(dbContext);
        var result = await sut.Execute();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
