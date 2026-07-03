using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Module.Auth.Application.UseCases.Users.CreateUser;
using Module.Auth.Domain;
using Common.Contracts.branches;
using Common.Contracts.branches.dtos;

namespace Test.Auth;

public class CreateUserTests
{
    [Fact]
    public async Task Execute_ShouldCreateUser_WithVerificationCode()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        dbContext.Branches.Add(new Branch { Id = branchId, Name = "Branch", Place = "X", PhoneNumber = "123", CreatedAt = DateTime.UtcNow });
        dbContext.Roles.Add(new Role { Id = roleId, Name = "Employee" });
        await dbContext.SaveChangesAsync();

        var branchServiceMock = new Mock<IBranchService>();
        branchServiceMock
            .Setup(s => s.GetBranchesByIds(new List<Guid> { branchId }))
            .ReturnsAsync(new List<BranchDto> { new() { Id = branchId, Name = "Branch" } });

        var sut = new CreateUser(dbContext, branchServiceMock.Object, tenantContext);

        var request = new CreateUserRequest
        {
            Email = "employee@test.com",
            Username = "employee",
            BranchRoles = [new UserBranchRoleDto { BranchId = branchId, RoleId = roleId }],
        };

        var result = await sut.Execute(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

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

        var savedCode = await dbContext.EmailVerificationCodes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.UserId == savedUser.Id);

        Assert.NotNull(savedCode);
        Assert.Equal(result.Value, savedCode.Code);
        Assert.Equal(savedUser.Id, savedCode.UserId);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenEmailTaken()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            Username = "existing",
            Status = UserStatus.Active,
        });
        await dbContext.SaveChangesAsync();

        var branchServiceMock = new Mock<IBranchService>();
        var sut = new CreateUser(dbContext, branchServiceMock.Object, tenantContext);

        var request = new CreateUserRequest
        {
            Email = "existing@test.com",
            Username = "newuser",
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

        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        dbContext.Branches.Add(new Branch { Id = branchId, Name = "Branch", Place = "X", PhoneNumber = "123", CreatedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();

        var branchServiceMock = new Mock<IBranchService>();
        branchServiceMock
            .Setup(s => s.GetBranchesByIds(new List<Guid> { branchId }))
            .ReturnsAsync(new List<BranchDto> { new() { Id = branchId, Name = "Branch" } });

        var sut = new CreateUser(dbContext, branchServiceMock.Object, tenantContext);

        var request = new CreateUserRequest
        {
            Email = "employee@test.com",
            Username = "employee",
            BranchRoles = [new UserBranchRoleDto { BranchId = branchId, RoleId = roleId }],
        };

        var result = await sut.Execute(request);

        Assert.Equal(CreateUserErrors.RolesNotFound, result.Error);
    }
}
