using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Users.UpdateUser;
using Module.Auth.Domain;

namespace Test.Auth.Users;

public class UpdateUserTests
{
    private static UpdateUser CreateSut(IAuthDbContext dbContext)
    {
        return new UpdateUser(dbContext);
    }

    private static (Guid TenantId, Guid BranchId, Guid RoleId, Guid UserId) Seed(IAuthDbContext dbContext, Guid tenantId)
    {
        var branchId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var databaseId = Guid.NewGuid();
        dbContext.TenantDatabases.Add(new TenantDataBase
        {
            Id = databaseId, Name = "TestDB", Description = "Test", Schema = "test",
        });

        var planId = Guid.NewGuid();
        dbContext.Plans.Add(new Plan { Id = planId, Name = "Basic", DefaultRolesTemplate = [] });

        dbContext.Tenants.Add(new Tenant
        {
            Id = tenantId, DisplayName = "Test", DataBaseId = databaseId, PlanId = planId, OwnerId = Guid.NewGuid(),
        });

        dbContext.Branches.Add(new Branch { Id = branchId, Name = "Branch", Place = "X", PhoneNumber = "123", CreatedAt = DateTime.UtcNow });
        dbContext.Roles.Add(new Role { Id = roleId, Name = "Employee" });
        dbContext.Users.Add(new User
        {
            Id = userId,
            Username = "Test-john",
            Email = "john@test.com",
            FirstName = "John",
            LastName = "Doe",
            Ci = "123",
            Nationality = "US",
            BirthDate = new DateTime(1990, 1, 1),
            Status = UserStatus.Ready,
            IsActive = true,
        });

        return (tenantId, branchId, roleId, userId);
    }

    [Fact]
    public async Task Execute_ShouldUpdatePersonalData_AndReplaceBranchRoles()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var (tenantId, branchId, roleId, userId) = Seed(dbContext, tenantContext.TenantId!.Value);
        var branchId2 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();
        dbContext.Branches.Add(new Branch { Id = branchId2, Name = "Branch2", Place = "Y", PhoneNumber = "456", CreatedAt = DateTime.UtcNow });
        dbContext.Roles.Add(new Role { Id = roleId2, Name = "Manager" });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext);

        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Ci = "456",
            Nationality = "UK",
            BirthDate = new DateTime(1995, 5, 5),
            BranchRoles =
            [
                new BranchRoleDto { BranchId = branchId2, RoleId = roleId2 },
            ],
        };

        var result = await sut.Execute(userId, request);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);

        var saved = await dbContext.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserBranchRoles)
            .FirstAsync(u => u.Id == userId);

        Assert.Equal("Jane", saved.FirstName);
        Assert.Equal("Smith", saved.LastName);
        Assert.Equal("456", saved.Ci);
        Assert.Equal("UK", saved.Nationality);
        Assert.Equal(new DateTime(1995, 5, 5), saved.BirthDate);
        Assert.NotNull(saved.UpdatedAt);

        Assert.Single(saved.UserBranchRoles);
        Assert.Equal(branchId2, saved.UserBranchRoles.First().BranchId);
        Assert.Equal(roleId2, saved.UserBranchRoles.First().RoleId);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenUserNotFound()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var sut = CreateSut(dbContext);
        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            BranchRoles = [new BranchRoleDto { BranchId = Guid.NewGuid(), RoleId = Guid.NewGuid() }],
        };

        var result = await sut.Execute(Guid.NewGuid(), request);

        Assert.Equal(UpdateUserErrors.UserNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenBranchNotFound()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var (tenantId, branchId, roleId, userId) = Seed(dbContext, tenantContext.TenantId!.Value);
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext);
        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            BranchRoles = [new BranchRoleDto { BranchId = Guid.NewGuid(), RoleId = roleId }],
        };

        var result = await sut.Execute(userId, request);

        Assert.Equal(UpdateUserErrors.BranchesNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenRoleNotFound()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var (tenantId, branchId, roleId, userId) = Seed(dbContext, tenantContext.TenantId!.Value);
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext);
        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            BranchRoles = [new BranchRoleDto { BranchId = branchId, RoleId = Guid.NewGuid() }],
        };

        var result = await sut.Execute(userId, request);

        Assert.Equal(UpdateUserErrors.RolesNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldMixKeepRemoveAndAdd()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var (tenantId, branchId, roleId, userId) = Seed(dbContext, tenantContext.TenantId!.Value);
        var branch2 = Guid.NewGuid();
        var role2 = Guid.NewGuid();
        var branch3 = Guid.NewGuid();
        var role3 = Guid.NewGuid();
        dbContext.Branches.Add(new Branch { Id = branch2, Name = "B2", Place = "Y", PhoneNumber = "456", CreatedAt = DateTime.UtcNow });
        dbContext.Branches.Add(new Branch { Id = branch3, Name = "B3", Place = "Z", PhoneNumber = "789", CreatedAt = DateTime.UtcNow });
        dbContext.Roles.Add(new Role { Id = role2, Name = "Manager" });
        dbContext.Roles.Add(new Role { Id = role3, Name = "Viewer" });

        var user = dbContext.Users.Local.First(u => u.Id == userId);
        user.UserBranchRoles =
        [
            new UserBranchRole { UserId = userId, BranchId = branchId, RoleId = roleId },
            new UserBranchRole { UserId = userId, BranchId = branch2, RoleId = role2 },
        ];
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext);

        var request = new UpdateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            BranchRoles =
            [
                new BranchRoleDto { BranchId = branchId, RoleId = roleId },   // keep
                new BranchRoleDto { BranchId = branch3, RoleId = role3 },     // add
            ],
        };

        var result = await sut.Execute(userId, request);

        Assert.True(result.IsSuccess);

        var saved = await dbContext.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserBranchRoles)
            .FirstAsync(u => u.Id == userId);

        Assert.Equal(2, saved.UserBranchRoles.Count);
        Assert.Contains(saved.UserBranchRoles, r => r.BranchId == branchId && r.RoleId == roleId);
        Assert.Contains(saved.UserBranchRoles, r => r.BranchId == branch3 && r.RoleId == role3);
    }

    [Fact]
    public async Task Execute_ShouldOnlyUpdateProvidedFields_WhenOthersAreNull()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var (tenantId, branchId, roleId, userId) = Seed(dbContext, tenantContext.TenantId!.Value);
        var user = dbContext.Users.Local.First(u => u.Id == userId);
        user.UserBranchRoles =
        [
            new UserBranchRole { UserId = userId, BranchId = branchId, RoleId = roleId },
        ];
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext);

        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = null,
            Ci = null,
            Nationality = null,
            BirthDate = null,
            BranchRoles = null,
        };

        var result = await sut.Execute(userId, request);

        Assert.True(result.IsSuccess);

        var saved = await dbContext.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserBranchRoles)
            .FirstAsync(u => u.Id == userId);

        Assert.Equal("Jane", saved.FirstName);
        Assert.Equal("Doe", saved.LastName);
        Assert.Equal("123", saved.Ci);
        Assert.Equal("US", saved.Nationality);
        Assert.Equal(new DateTime(1990, 1, 1), saved.BirthDate);
        Assert.Single(saved.UserBranchRoles);
        Assert.Equal(branchId, saved.UserBranchRoles.First().BranchId);
    }
}
