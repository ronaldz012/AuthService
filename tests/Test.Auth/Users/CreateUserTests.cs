using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Users.CreateUser;
using Module.Auth.Domain;
using Module.Auth.Infrastructure.Authentication;

namespace Test.Auth.Users;

public class CreateUserTests
{
    private static CreateUser CreateSut(IAuthDbContext dbContext, ITenantConnectionContext tenantConnectionContext, ICurrentUser? currentUser = null)
    {
        currentUser ??= Mock.Of<ICurrentUser>(u => u.UserId == Guid.NewGuid() && u.FullName == "Test User");
        return new CreateUser(
            dbContext,
            tenantConnectionContext,
            currentUser,
            Mock.Of<IEmailVerificationService>(),
            Options.Create(new ProjectInfo
            {
                AppBranding = new AppBranding { FrontendDomain = "test.com" },
                EmailTemplateDefaults = new EmailTemplateDefaults()
            }));
    }

    private static void SeedTenant(IAuthDbContext dbContext, Guid tenantId, string displayName = "TestTenant")
    {
        var databaseId = Guid.NewGuid();
        dbContext.TenantDatabases.Add(new TenantDataBase
        {
            Id = databaseId,
            Name = "TestDB",
            Description = "Test database",
            Schema = "test",
        });

        var planId = Guid.NewGuid();
        dbContext.Plans.Add(new Plan
        {
            Id = planId,
            Name = "Basic",
            DefaultRolesTemplate = [],
        });

        dbContext.Tenants.Add(new Tenant
        {
            Id = tenantId,
            DisplayName = displayName,
            DataBaseId = databaseId,
            PlanId = planId,
            OwnerId = Guid.NewGuid(),
        });
    }

    [Fact]
    public async Task Execute_ShouldCreateUser_WithVerificationCode()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        SeedTenant(dbContext, tenantId);
        dbContext.Branches.Add(new Branch { Id = branchId, Name = "Branch", Place = "X", PhoneNumber = "123", CreatedAt = DateTime.UtcNow });
        dbContext.Roles.Add(new Role { Id = roleId, Name = "Employee" });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext, tenantContext);

        var request = new CreateUserRequest
        {
            Email = "employee@test.com",
            Username = "employee",
            FirstName = "John",
            LastName = "Doe",
            BranchRoles = [new UserBranchRoleDto { BranchId = branchId, RoleId = roleId }],
        };

        var result = await sut.Execute(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.UserId);
        Assert.NotEmpty(result.Value.SetupUrl);
        Assert.Contains("/auth/setup-password?code=", result.Value.SetupUrl);

        var savedUser = await dbContext.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserBranchRoles)
            .FirstOrDefaultAsync(u => u.Email == "employee@test.com");

        Assert.NotNull(savedUser);
        Assert.Equal(UserStatus.PendingPasswordSetup, savedUser.Status);
        Assert.Equal(UserType.Standard, savedUser.Type);
        Assert.Equal(tenantId, savedUser.TenantId);
        Assert.Single(savedUser.UserBranchRoles);
        Assert.Equal(branchId, savedUser.UserBranchRoles.First().BranchId);
        Assert.Equal("John", savedUser.FirstName);
        Assert.Equal("Doe", savedUser.LastName);
        Assert.Equal("TestTenant-employee", savedUser.Username);

        var savedCode = await dbContext.EmailVerificationCodes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.UserId == savedUser.Id);

        Assert.NotNull(savedCode);
        Assert.Equal(savedUser.Id, savedCode.UserId);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenEmailTaken()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        SeedTenant(dbContext, tenantId);
        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            Username = "existing",
            TenantId = tenantId,
            Status = UserStatus.Ready,
            IsActive = true,
        });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext, tenantContext);

        var request = new CreateUserRequest
        {
            Email = "existing@test.com",
            Username = "newuser",
            FirstName = "Jane",
            LastName = "Smith",
            BranchRoles = [],
        };

        var result = await sut.Execute(request);

        Assert.Equal(CreateUserErrors.EmailOrUsernameTaken, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenUsernameTaken()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        SeedTenant(dbContext, tenantId, "TestTenant");
        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "other@test.com",
            Username = "TestTenant-taken",
            TenantId = tenantId,
            Status = UserStatus.Ready,
            IsActive = true,
        });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext, tenantContext);

        var request = new CreateUserRequest
        {
            Email = "new@test.com",
            Username = "taken",
            FirstName = "Jane",
            LastName = "Smith",
            BranchRoles = [],
        };

        var result = await sut.Execute(request);

        Assert.Equal(CreateUserErrors.EmailOrUsernameTaken, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenRoleNotFound()
    {
        var branchId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var tenantId = Guid.NewGuid();
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        SeedTenant(dbContext, tenantId);
        dbContext.Branches.Add(new Branch { Id = branchId, Name = "Branch", Place = "X", PhoneNumber = "123", CreatedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext, tenantContext);

        var request = new CreateUserRequest
        {
            Email = "employee@test.com",
            Username = "employee",
            FirstName = "John",
            LastName = "Doe",
            BranchRoles = [new UserBranchRoleDto { BranchId = branchId, RoleId = roleId }],
        };

        var result = await sut.Execute(request);

        Assert.Equal(CreateUserErrors.MissingRoles, result.Error);
    }
}
