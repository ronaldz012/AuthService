using System;
using Auth.Data.Entities;

namespace Auth.Infrastructure.Email;

public interface IEmailVerificationService
{
    Task SendVerificationEmailAsync(int userId, string userEmail, VerificationCodePurpose purpose);
    Task<bool> ValidateVerificationCodeAsync(int userId, string code, VerificationCodePurpose purpose);
    Task ResendVerificationEmailAsync(int userId, string userEmail, VerificationCodePurpose purpose);
}
