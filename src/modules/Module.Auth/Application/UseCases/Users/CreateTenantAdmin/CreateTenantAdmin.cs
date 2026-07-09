using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;
using Module.Auth.Infrastructure.Authentication;

namespace Module.Auth.Application.UseCases.Users.CreateTenantAdmin;

public class CreateTenantAdmin(
    IAuthDbContext context,
    ITenantContext tenantContext,
    IEmailVerificationService emailVerificationService,
    IOptions<ProjectInfo> projectInfo)
{
    public async Task<Result<CreateTenantAdminResponse>> Execute(CreateTenantAdminRequest dto)
    {
        var displayName = await context.Tenants
            .Where(t => t.Id == tenantContext.TenantId)
            .Select(t => t.DisplayName)
            .FirstAsync();

        var globalUsername = $"{displayName}-{dto.Username}";

        var usernameTaken = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == globalUsername);
        if (usernameTaken) return CreateTenantAdminErrors.EmailOrUsernameTaken;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailTaken = await context.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == dto.Email);
            if (emailTaken) return CreateTenantAdminErrors.EmailOrUsernameTaken;
        }

        var newUser = User.CreateTenantAdmin(
            dto.Email, globalUsername, dto.FirstName, dto.LastName,
            dto.Ci, dto.Nationality, dto.BirthDate);

        var verificationCode = EmailVerificationCode.CreateForAccountSetup(dto.Email ?? string.Empty);
        newUser.EmailVerificationCodes.Add(verificationCode);
        context.Users.Add(newUser);

        await context.SaveChangesAsync();

        var frontendDomain = projectInfo.Value.AppBranding.FrontendDomain;
        var setupUrl = $"https://{frontendDomain}/auth/setup-password?code={verificationCode.Code}";

        var emailSent = false;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            try
            {
                await emailVerificationService.SendTenantSetupEmailAsync(
                    dto.Email,
                    dto.Username,
                    setupUrl,
                    verificationCode.ExpiresAt);
                emailSent = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return new CreateTenantAdminResponse(newUser.Id, setupUrl, emailSent);
    }
}
