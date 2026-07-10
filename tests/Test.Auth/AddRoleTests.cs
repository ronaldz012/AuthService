using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.UseCases.Roles.Create;
using Module.Auth.Domain;

namespace Test.Auth;

public class AddRoleTests
{
    [Fact]
    public async Task Execute_ShouldCreateRole_WithTenantIdFromContext()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var sut = new AddRole(dbContext);

        var dto = new CreateRoleDto
        {
            Name = "Admin",
            Description = "Administrator role",
            RoleModulePermissions =
            [
                new RoleFeaturePermissionDto
                {
                    FeatureKey = "dashboard",
                    Permissions = ["view", "edit"],
                },
                new RoleFeaturePermissionDto
                {
                    FeatureKey = "users",
                    Permissions = ["view"],
                },
            ],
        };

        var result = await sut.Execute(dto);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var savedRole = await dbContext.Roles
            .IgnoreQueryFilters()
            .Include(r => r.RoleFeaturePermissions)
            .FirstOrDefaultAsync(r => r.Id == result.Value);

        Assert.NotNull(savedRole);
        Assert.Equal("Admin", savedRole.Name);
        Assert.Equal(tenantId, savedRole.TenantId);
        Assert.Equal(2, savedRole.RoleFeaturePermissions.Count);

        foreach (var perm in savedRole.RoleFeaturePermissions)
        {
            Assert.Equal(tenantId, perm.TenantId);
        }

        var dashboardPerm = savedRole.RoleFeaturePermissions
            .First(p => p.FeatureKey == "dashboard");
        Assert.Equal(["view", "edit"], dashboardPerm.Permissions);
    }
}
