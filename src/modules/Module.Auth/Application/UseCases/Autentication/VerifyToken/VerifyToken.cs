using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Autentication.VerifyToken;

public class VerifyToken(IAuthDbContext context)
{
    public async Task<Result<VerifyTokenResponse>> ExecuteAsync(string token)
    {
        var verificationCode = await context.EmailVerificationCodes
            .FirstOrDefaultAsync(c => c.Code == token && !c.IsUsed);

        if (verificationCode == null)
            return new VerifyTokenResponse { Valid = false };

        if (verificationCode.ExpiresAt < DateTime.UtcNow)
            return new VerifyTokenResponse { Valid = false };

        return new VerifyTokenResponse
        {
            Valid = true,
            Email = verificationCode.Email,
        };
    }
}
