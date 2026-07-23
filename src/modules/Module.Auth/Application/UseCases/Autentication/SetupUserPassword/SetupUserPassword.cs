using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Autentication.SetupUserPassword;

public class SetupUserPassword(IAuthDbContext context, ITenantConnectionContext tenantConnectionContext)
{
    public async Task<Result<bool>> ExecuteAsync(SetupUserPasswordRequest request)
    {
        var verificationCode = await context.EmailVerificationCodes.IgnoreQueryFilters()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Code == request.Token && !c.IsUsed);

        if (verificationCode == null)
            return SetupUserPasswordErrors.TokenNotFound;

        tenantConnectionContext.TenantId = verificationCode.User.TenantId;

        if (verificationCode.IsExpired)
            return SetupUserPasswordErrors.TokenExpired;

        var ownerUser = verificationCode.User;

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            ownerUser.SetPassword(BCrypt.Net.BCrypt.HashPassword(request.Password));
            verificationCode.MarkAsUsed();

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