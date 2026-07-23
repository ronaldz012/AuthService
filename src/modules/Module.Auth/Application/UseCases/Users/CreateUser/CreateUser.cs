using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.CreateUser;

public class CreateUser(
    IAuthDbContext context,
    ITenantConnectionContext tenantConnectionContext,
    ICurrentUser currentUser,
    IEmailVerificationService emailVerificationService,
    IOptions<ProjectInfo> projectInfo)
{
    public async Task<Result<CreateUserResponse>> Execute(CreateUserRequest dto)
    {
        var displayName = await context.Tenants
            .Where(t => t.Id == tenantConnectionContext.TenantId)
            .Select(t => t.DisplayName)
            .FirstAsync();

        var globalUsername = $"{displayName}-{dto.Username}";

        var usernameTaken = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == globalUsername);
        if (usernameTaken) return CreateUserErrors.EmailOrUsernameTaken;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailTaken = await context.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == dto.Email);
            if (emailTaken) return CreateUserErrors.EmailOrUsernameTaken;
        }

        var branchIds = dto.BranchRoles.Select(br => br.BranchId).Distinct().ToList();
        var roleIds = dto.BranchRoles.Select(br => br.RoleId).Distinct().ToList();

        var foundBranchIds = await context.Branches
            .Where(b => branchIds.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync();

        if (foundBranchIds.Count != branchIds.Count)
            return CreateUserErrors.BranchesNotFound;

        var rolesResult = await ValidateRoles(roleIds);
        if (!rolesResult.IsSuccess)
            return CreateUserErrors.MissingRoles;

        var newUser = User.CreateStandard(dto.Email, globalUsername, dto.FirstName, dto.LastName, dto.Ci, dto.Nationality, dto.BirthDate, currentUser.UserId, currentUser.FullName);
        newUser.UserBranchRoles = dto.BranchRoles.Select(br => UserBranchRole.Create(newUser.Id, br.BranchId, br.RoleId, currentUser.UserId, currentUser.FullName)).ToList();

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
            catch (Exception)
            {
            }
        }

        return new CreateUserResponse(newUser.Id, setupUrl, emailSent);
    }

    private async Task<Result<bool>> ValidateRoles(List<Guid> roleIds)
    {
        var foundRolesIds = await context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync();

        var missingRoleIds = roleIds.Except(foundRolesIds).ToList();

        if (missingRoleIds.Any())
            return CreateUserErrors.MissingRoles;

        return true;
    }
}
