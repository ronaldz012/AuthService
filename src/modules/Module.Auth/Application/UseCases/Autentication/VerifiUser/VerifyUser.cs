using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Autentication.VerifiUser;

public class VerifyUser(IAuthDbContext dbContext )
{
    public async Task<Result<bool>> Execute(string verifyCode)
    {
        EmailVerificationCode? code = dbContext.EmailVerificationCodes.Include(u => u.User).FirstOrDefault(c => c.Code == verifyCode
                                                                                            && !c.IsUsed
                                                                                            && c.Purpose == VerificationCodePurpose.AccountVerification);
        if (code == null)
            return VerifyUserErrors.CodeNotFound;
        if (code.ExpiresAt < DateTime.UtcNow)
            return VerifyUserErrors.CodeExpired;
        code.IsUsed = true;
        code.User.Status = UserStatus.Active;
        await dbContext.SaveChangesAsync();
        return true;
    } 
}
