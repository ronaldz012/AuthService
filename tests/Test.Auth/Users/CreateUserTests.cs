using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Users.CreateUser;
using Module.Auth.Domain;
using Moq;

namespace Test.Auth.Users;

public class CreateUserTests
{
    private static ActorContext CreateActorContext(Guid tenantId)
        => new(tenantId, Guid.NewGuid(), "Test User", Guid.Empty, []);

    private static CreateUser CreateSut(
        IAuthDbContext dbContext,
        ITenantConnectionContext tenantConnectionContext,
        IAuth0ProvisioningService? auth0 = null,
        IOptions<ProjectInfo>? projectInfo = null)
    {
        var auth0Mock = auth0 ?? Mock.Of<IAuth0ProvisioningService>(a =>
            a.CreateInvitationUserAsync(It.IsAny<string>()) == Task.FromResult<Result<string>>("auth0|123") &&
            a.CreatePasswordChangeTicketAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()) == Task.FromResult<Result<string>>("https://ticket.test/setup"));

        var projMock = projectInfo ?? Options.Create(new ProjectInfo
        {
            AppBranding = new AppBranding { FrontendDomain = "test.example.com" }
        });

        return new CreateUser(dbContext, tenantConnectionContext, auth0Mock, projMock, Mock.Of<ILogger<CreateUser>>());
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
    public async Task Execute_ShouldCreateUser_WithSetupTicket()
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
            FirstName = "John",
            LastName = "Doe",
            BranchRoles = [new UserBranchRoleDto { BranchId = branchId, RoleId = roleId }],
        };

        var result = await sut.Execute(CreateActorContext(tenantId), request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.UserId);
        Assert.Equal("https://ticket.test/setup", result.Value.SetupUrl);

        var savedUser = await dbContext.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserBranchRoles)
            .FirstOrDefaultAsync(u => u.Email == "employee@test.com");

        Assert.NotNull(savedUser);
        Assert.Equal(UserStatus.PendingPasswordSetup, savedUser.Status);
        Assert.Equal(UserType.Standard, savedUser.Type);
        Assert.Equal(tenantId, savedUser.TenantId);
        Assert.Equal("auth0|123", savedUser.ExternalAuthId);
        Assert.Equal(AuthProvider.Auth0, savedUser.AuthProvider);
        Assert.Equal("https://ticket.test/setup", savedUser.PasswordChangeTicket);
        Assert.NotNull(savedUser.PasswordChangeTicketExpiresAt);
        Assert.Equal("employee@test.com", savedUser.Username);
        Assert.Single(savedUser.UserBranchRoles);
        Assert.Equal(branchId, savedUser.UserBranchRoles.First().BranchId);
        Assert.Equal("John", savedUser.FirstName);
        Assert.Equal("Doe", savedUser.LastName);
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
            Username = "existing@test.com",
            TenantId = tenantId,
            Status = UserStatus.Ready,
            IsActive = true,
        });
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext, tenantContext);

        var request = new CreateUserRequest
        {
            Email = "existing@test.com",
            FirstName = "Jane",
            LastName = "Smith",
            BranchRoles = [],
        };

        var result = await sut.Execute(CreateActorContext(tenantId), request);

        Assert.Equal(CreateUserErrors.EmailOrUsernameTaken, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenEmailTaken_GloballyAcrossTenants()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var ctxA = TestAuthDbContextFactory.CreateTenantContext(tenantA);
        var ctxB = TestAuthDbContextFactory.CreateTenantContext(tenantB);
        var dbName = $"AuthEmailGlobal_{Guid.NewGuid()}";
        using var dbA = TestAuthDbContextFactory.Create(ctxA, dbName);
        using var dbB = TestAuthDbContextFactory.Create(ctxB, dbName);

        SeedTenant(dbA, tenantA, "TenantA");
        SeedTenant(dbB, tenantB, "TenantB");
        // need to save tenant in same db - use dbA for seed
        await dbA.SaveChangesAsync();
        // Actually tenants are in same underlying InMemory DB when dbName shared, so we need to add via one context
        // Simpler: add user in tenantA, then try to create same email in tenantB
        dbA.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "shared@test.com",
            Username = "shared@test.com",
            TenantId = tenantA,
            Status = UserStatus.Ready,
            IsActive = true,
        });
        await dbA.SaveChangesAsync();

        var sut = CreateSut(dbB, ctxB);

        var request = new CreateUserRequest
        {
            Email = "shared@test.com",
            FirstName = "Jane",
            LastName = "Smith",
            BranchRoles = [],
        };

        var result = await sut.Execute(CreateActorContext(tenantB), request);

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
            FirstName = "John",
            LastName = "Doe",
            BranchRoles = [new UserBranchRoleDto { BranchId = branchId, RoleId = roleId }],
        };

        var result = await sut.Execute(CreateActorContext(tenantId), request);

        Assert.Equal(CreateUserErrors.MissingRoles, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenAuth0Fails()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);
        SeedTenant(dbContext, tenantId);
        await dbContext.SaveChangesAsync();

        var auth0Mock = new Mock<IAuth0ProvisioningService>();
        auth0Mock.Setup(a => a.CreateInvitationUserAsync(It.IsAny<string>()))
            .ReturnsAsync(new Error(ErrorCode.InternalError, "Auth0 down"));

        var sut = CreateSut(dbContext, tenantContext, auth0Mock.Object);

        var request = new CreateUserRequest
        {
            Email = "fail@test.com",
            FirstName = "John",
            LastName = "Doe",
            BranchRoles = [],
        };

        var result = await sut.Execute(CreateActorContext(tenantId), request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InternalError, result.Error.Code);
    }
}
