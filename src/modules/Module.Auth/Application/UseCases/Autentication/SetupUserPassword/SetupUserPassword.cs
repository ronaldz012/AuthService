using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Autentication.SetupUserPassword;

public class SetupUserPassword(IAuthDbContext  context)
{
    public async Task<Result<bool>> ExecuteAsync(SetupUserPasswordRequest request)
    {
        // 1. Buscar el código de verificación e incluir al usuario dueño
        var verificationCode = await context.EmailVerificationCodes
            .FirstOrDefaultAsync(c => c.Code == request.Token && !c.IsUsed);

        if (verificationCode == null)
            return new Error("NOT_FOUND", "The verification token is invalid or has already been used.");

        if (verificationCode.ExpiresAt < DateTime.UtcNow)
            return new Error("VALIDATION_ERROR", "The verification token has expired.");

        var ownerUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == verificationCode.UserId && u.TenantId == verificationCode.TenantId);

        if (ownerUser == null)
            return new Error("NOT_FOUND", "The associated owner user was not found.");

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            ownerUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            ownerUser.Status = UserStatus.Active; 
            ownerUser.UpdatedAt = DateTime.UtcNow;
            verificationCode.IsUsed = true;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}