using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.UseCases.Roles.Create;
using Module.Auth.Application.UseCases.Roles.GetById;
using Module.Auth.Domain;

namespace Test.Auth;

public class GetRoleTests
{
    [Fact]
    public async Task Execute_ShouldReturnRole_WhenFound()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var roleId = Guid.NewGuid();
        dbContext.Roles.Add(new Role
        {
            Id = roleId,
            Name = "Manager",
            Description = "Manager role",
        });
        await dbContext.SaveChangesAsync();

        var sut = new GetRoleById(dbContext);
        var result = await sut.Execute(roleId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(roleId, result.Value.Id);
        Assert.Equal("Manager", result.Value.Name);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenRoleNotFound()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var sut = new GetRoleById(dbContext);
        var result = await sut.Execute(Guid.NewGuid());

        Assert.Equal(GetRoleByIdErrors.RoleNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturn_OnlyOwnTenantRoles()
    {
        var dbName = $"GetRole_{Guid.NewGuid()}";
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var roleAId = Guid.NewGuid();
        var roleBId = Guid.NewGuid();

        var contextA = TestAuthDbContextFactory.CreateTenantContext(tenantAId);
        using (var dbA = TestAuthDbContextFactory.Create(contextA, dbName))
        {
            dbA.Roles.Add(new Role { Id = roleAId, Name = "Role A" });
            await dbA.SaveChangesAsync();
        }

        var contextB = TestAuthDbContextFactory.CreateTenantContext(tenantBId);
        using (var dbB = TestAuthDbContextFactory.Create(contextB, dbName))
        {
            dbB.Roles.Add(new Role { Id = roleBId, Name = "Role B" });
            await dbB.SaveChangesAsync();
        }

        using (var dbReadA = TestAuthDbContextFactory.Create(contextA, dbName))
        {
            var sutA = new GetRoleById(dbReadA);
            var resultA = await sutA.Execute(roleAId);
            Assert.True(resultA.IsSuccess);
            Assert.Equal("Role A", resultA.Value.Name);

            var resultB = await sutA.Execute(roleBId);
            Assert.Equal(GetRoleByIdErrors.RoleNotFound, resultB.Error);
        }

        using (var dbReadB = TestAuthDbContextFactory.Create(contextB, dbName))
        {
            var sutB = new GetRoleById(dbReadB);
            var resultB = await sutB.Execute(roleBId);
            Assert.True(resultB.IsSuccess);
            Assert.Equal("Role B", resultB.Value.Name);

            var resultA = await sutB.Execute(roleAId);
            Assert.Equal(GetRoleByIdErrors.RoleNotFound, resultA.Error);
        }

        using (var dbAll = TestAuthDbContextFactory.Create(contextA, dbName))
        {
            var allRoles = await dbAll.Roles
                .IgnoreQueryFilters()
                .ToListAsync();
            Assert.Equal(2, allRoles.Count);
        }
    }
}
