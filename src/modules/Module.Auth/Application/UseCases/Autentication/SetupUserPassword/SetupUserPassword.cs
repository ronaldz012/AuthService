using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Autentication.SetupUserPassword;

public class SetupUserPassword(IAuthDbContext  context, ITenantContext tenantContext)
{
    public async Task<Result<bool>> ExecuteAsync(SetupUserPasswordRequest request)
    {
        // 1. Buscar el código de verificación e incluir al usuario dueño
        var verificationCode = await context.EmailVerificationCodes.IgnoreQueryFilters()
        .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Code == request.Token && !c.IsUsed);

        if (verificationCode == null)
            return SetupUserPasswordErrors.TokenNotFound;

        tenantContext.TenantId = verificationCode.User.TenantId;

        if (verificationCode.ExpiresAt < DateTime.UtcNow)
            return SetupUserPasswordErrors.TokenExpired;

        var ownerUser = verificationCode.User;

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            ownerUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            ownerUser.Status = UserStatus.Ready;
            ownerUser.IsActive = true;
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