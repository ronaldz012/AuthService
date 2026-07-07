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

        var sessionState = new Mock<ISessionStateService>();
        sessionState
            .Setup(s => s.GetOrBuildAsync(userId, tenantId, false))
            .ReturnsAsync(new SessionStateDto(
                new UserDetailResponse
                {
                    Id = userId,
                    Email = "active@test.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    UserType = (int)UserType.Standard,
                },
                [],
                new TenantPlanUsageDto("Free", [], 10, 1, 5, 1)
            ));

        var sut = CreateSut(dbContext, tenantContext, sessionState.Object);

        var request = new LoginRequest { Email = "active@test.com", Password = "Password123!" };
        var result = await sut.Execute(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("fake-access-token", result.Value.AccessToken);
        Assert.Equal("fake-refresh-token", result.Value.RefreshToken);
        Assert.Equal(3600, result.Value.ExpiresIn);
    }

    private static Login CreateSut(IAuthDbContext dbContext, ITenantContext tenantContext,
        ISessionStateService? sessionState = null)
    {
        sessionState ??= new Mock<ISessionStateService>().Object;
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
            sessionState,
            tokenGeneratorMock.Object,
            new Mock<ILogger<Login>>().Object);
    }
}
