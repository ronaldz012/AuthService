using Module.Auth.Application.UseCases.Autentication.VerifyToken;
using Module.Auth.Domain;

namespace Test.Auth.Authentication;

public class VerifyTokenTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidFalse_WhenTokenNotFound()
    {
        using var dbContext = TestAuthDbContextFactory.Create();
        var sut = new VerifyToken(dbContext);

        var result = await sut.ExecuteAsync("nonexistent-token");

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.Valid);
        Assert.Null(result.Value.Email);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidFalse_WhenTokenExpired()
    {
        using var dbContext = TestAuthDbContextFactory.Create();

        dbContext.EmailVerificationCodes.Add(new EmailVerificationCode
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Code = "expired-token",
            Email = "user@test.com",
            SentAt = DateTime.UtcNow.AddDays(-3),
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            Purpose = VerificationCodePurpose.AccountVerification,
            IsUsed = false,
        });
        await dbContext.SaveChangesAsync();

        var sut = new VerifyToken(dbContext);

        var result = await sut.ExecuteAsync("expired-token");

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.Valid);
        Assert.Null(result.Value.Email);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidTrue_WithEmail_WhenTokenValid()
    {
        using var dbContext = TestAuthDbContextFactory.Create();

        var code = new EmailVerificationCode
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Code = "valid-token",
            Email = "user@test.com",
            SentAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            Purpose = VerificationCodePurpose.AccountVerification,
            IsUsed = false,
        };
        dbContext.EmailVerificationCodes.Add(code);
        await dbContext.SaveChangesAsync();

        var sut = new VerifyToken(dbContext);

        var result = await sut.ExecuteAsync("valid-token");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Valid);
        Assert.Equal("user@test.com", result.Value.Email);
    }
}
