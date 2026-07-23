using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Autentication.VerifiUser;

public class VerifyUser(IAuthDbContext dbContext)
{
    public async Task<Result<bool>> Execute(string verifyCode)
    {
        EmailVerificationCode? code = await dbContext.EmailVerificationCodes
            .Include(u => u.User)
            .FirstOrDefaultAsync(c => c.Code == verifyCode
                                   && !c.IsUsed
                                   && c.Purpose == VerificationCodePurpose.AccountVerification);

        if (code == null)
            return VerifyUserErrors.CodeNotFound;

        if (code.IsExpired)
            return VerifyUserErrors.CodeExpired;

        code.MarkAsUsed();
        code.User.MarkAsVerified();

        await dbContext.SaveChangesAsync();
        return true;
    }
}
