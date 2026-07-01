using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Tenant.CompleteTenant;
using Module.Auth.Domain;
using Module.Auth.Infrastructure.Authentication;
using Module.Auth.Infrastructure.Persistence;

namespace Test.Auth;

public class CompleteTenantTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenTokenNotFound()
    {
        using var dbContext = CreateDbContext();
        var sut = new CompleteTenant(dbContext);

        var request = new CompleteTenantRequest("invalid-token", "Password123!");
        var result = await sut.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenTokenIsValid()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var code = "valid-token-123";

        using var dbContext = CreateDbContext();

        dbContext.EmailVerificationCodes.Add(new EmailVerificationCode
        {
            TenantId = tenantId,
            UserId = userId,
            Code = code,
            Email = "owner@test.com",
            SentAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddHours(47),
            Purpose = VerificationCodePurpose.AccountVerification,
            IsUsed = false,
        });

        dbContext.Users.Add(new User
        {
            Id = userId,
            TenantId = tenantId,
            Email = "owner@test.com",
            Username = "owner",
            PasswordHash = string.Empty,
            Status = UserStatus.PendingVerification,
            Type = UserType.Owner,
        });

        await dbContext.SaveChangesAsync();

        var sut = new CompleteTenant(dbContext);

        var request = new CompleteTenantRequest(code, "NewPassword123!");
        var result = await sut.ExecuteAsync(request);

        Assert.True(result.IsSuccess);

        var updatedUser = await dbContext.Users.FindAsync(userId);
        Assert.NotNull(updatedUser);
        Assert.Equal(UserStatus.Active, updatedUser.Status);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPassword123!", updatedUser.PasswordHash));

        var updatedCode = await dbContext.EmailVerificationCodes.FindAsync(1);
        Assert.NotNull(updatedCode);
        Assert.True(updatedCode.IsUsed);
    }

    private static AuthDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase($"CompleteTenantTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AuthDbContext(options, new TenantContext());
    }
}
