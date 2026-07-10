using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.UseCases.Roles.GetRoles;
using Module.Auth.Domain;

namespace Test.Auth;

public class GetRolesTests
{
    [Fact]
    public async Task Execute_ShouldReturn_OnlyOwnTenantRoles()
    {
        var dbName = $"GetRoles_{Guid.NewGuid()}";
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        var contextA = TestAuthDbContextFactory.CreateTenantContext(tenantAId);
        using (var dbA = TestAuthDbContextFactory.Create(contextA, dbName))
        {
            dbA.Roles.Add(new Role { Name = "Admin" });
            dbA.Roles.Add(new Role { Name = "Manager" });
            await dbA.SaveChangesAsync();
        }

        var contextB = TestAuthDbContextFactory.CreateTenantContext(tenantBId);
        using (var dbB = TestAuthDbContextFactory.Create(contextB, dbName))
        {
            dbB.Roles.Add(new Role { Name = "Viewer" });
            await dbB.SaveChangesAsync();
        }

        using (var dbReadA = TestAuthDbContextFactory.Create(contextA, dbName))
        {
            var sut = new GetRoles(dbReadA);
            var result = await sut.Execute();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Contains(result.Value, r => r.Name == "Admin");
            Assert.Contains(result.Value, r => r.Name == "Manager");
        }

        using (var dbReadB = TestAuthDbContextFactory.Create(contextB, dbName))
        {
            var sut = new GetRoles(dbReadB);
            var result = await sut.Execute();

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal("Viewer", result.Value[0].Name);
        }

        using (var dbAll = TestAuthDbContextFactory.Create(contextA, dbName))
        {
            var allRoles = await dbAll.Roles
                .IgnoreQueryFilters()
                .ToListAsync();
            Assert.Equal(3, allRoles.Count);
        }
    }

    [Fact]
    public async Task Execute_ShouldReturnEmpty_WhenNoRolesExist()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var sut = new GetRoles(dbContext);
        var result = await sut.Execute();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
