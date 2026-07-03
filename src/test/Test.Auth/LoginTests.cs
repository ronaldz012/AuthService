

using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.Utilities;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Autentication.Login;
using Module.Auth.Domain;
using Moq;

namespace Test.Auth;

public class LoginTests
{
    [Fact]
    public async Task Execute_ShouldReturnError_WhenUserNotFound()
    {
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext();
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);
        var sut = CreateSut(dbContext, tenantContext);

        var request = new LoginRequest { Email = "nonexistent@test.com", Password = "password123" };
        var result = await sut.Execute(request);

        Assert.Equal(LoginErrors.UserNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);
        dbContext.Users.Add(new User
        {
            Id = userId,
            Email = "active@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Status = UserStatus.Active,
            Type = UserType.Standard,
            FirstName = "Jane",
            LastName = "Smith",
            TenantId = tenantId,
            AuthProvider = AuthProvider.Local,
        });
        await dbContext.SaveChangesAsync();

        var permissionsCacheMock = new Mock<IUserPermissionsCacheService>();
        permissionsCacheMock
            .Setup(p => p.GetAsync(userId, tenantId, false))
            .ReturnsAsync(new List<PermissionsDto>
            {
                new()
                {
                    BranchId = Guid.NewGuid(),
                    BranchName = "Main",
                    RoleName = "Admin",
                    Features = new List<FeaturePermissionsDto>
                    {
                        new()
                        {
                            Key = "dashboard",
                            DisplayName = "Dashboard",
                            ModuleName = "Dashboard",
                            IsMenu = true,
                            Route = "/dashboard",
                            Permissions = new List<string> { "view" },
                        }
                    }
                }
            });

        var sut = CreateSut(dbContext, tenantContext, permissionsCacheMock.Object);

        var request = new LoginRequest { Email = "active@test.com", Password = "Password123!" };
        var result = await sut.Execute(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Active", result.Value.Status);
        Assert.Equal("fake-access-token", result.Value.AccessToken);
    }

    private static Login CreateSut(IAuthDbContext dbContext, ITenantContext tenantContext,
        IUserPermissionsCacheService? permissionsCache = null)
    {
        permissionsCache ??= new Mock<IUserPermissionsCacheService>().Object;
        var tokenGeneratorMock = new Mock<ITokenGenerator>();
        tokenGeneratorMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
            .Returns("fake-access-token");
        tokenGeneratorMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("fake-refresh-token");
        tokenGeneratorMock
            .Setup(t => t.GetExpirationMinutes())
            .Returns(60);

        return new Login(
            dbContext,
            tenantContext,
            permissionsCache,
            tokenGeneratorMock.Object,
            new Mock<ILogger<Login>>().Object);
    }
}
