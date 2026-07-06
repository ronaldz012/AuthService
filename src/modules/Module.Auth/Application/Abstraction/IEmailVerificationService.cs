using Module.Auth.Domain;

namespace Module.Auth.Application.Abstraction;

public interface IEmailVerificationService
{
    Task SendVerificationEmailAsync(User user, VerificationCodePurpose purpose);
    Task<bool> ValidateVerificationCodeAsync(int userId, string code, VerificationCodePurpose purpose);
    Task ResendVerificationEmailAsync(int userId, string userEmail, VerificationCodePurpose purpose);
    Task SendTenantSetupEmailAsync(string email, string userName, string setupLink, DateTime expiresAt);
}
