using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.CreateUser;

public class CreateUser(
    IAuthDbContext context,
    ITenantConnectionContext tenantConnectionContext,
    IAuth0ProvisioningService auth0Provisioning,
    IOptions<ProjectInfo> projectInfo,
    ILogger<CreateUser> logger)
{
    public async Task<Result<CreateUserResponse>> Execute(ActorContext ctx, CreateUserRequest dto)
    {
        var emailTaken = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == dto.Email);
        if (emailTaken) return CreateUserErrors.EmailOrUsernameTaken;

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

        // 1. Provision in Auth0
        var invitationResult = await auth0Provisioning.CreateInvitationUserAsync(dto.Email);
        if (!invitationResult.IsSuccess)
            return invitationResult.Error;
        var auth0Id = invitationResult.Value;

        var resultUrl = $"https://{projectInfo.Value.AppBranding.FrontendDomain}/login";
        var ticketResult = await auth0Provisioning.CreatePasswordChangeTicketAsync(auth0Id, resultUrl);
        if (!ticketResult.IsSuccess)
            return ticketResult.Error;
        var ticket = ticketResult.Value;
        var ticketExpiresAt = DateTime.UtcNow.AddSeconds(432000);

        var newUser = User.CreateStandard(dto.Email, dto.FirstName, dto.LastName, dto.Ci, dto.Nationality, dto.BirthDate, ctx.UserId, ctx.FullName);
        newUser.ExternalAuthId = auth0Id;
        newUser.AuthProvider = AuthProvider.Auth0;
        newUser.PasswordChangeTicket = ticket;
        newUser.PasswordChangeTicketExpiresAt = ticketExpiresAt;
        newUser.UserBranchRoles = dto.BranchRoles.Select(br => UserBranchRole.Create(newUser.Id, br.BranchId, br.RoleId, ctx.UserId, ctx.FullName)).ToList();

        context.Users.Add(newUser);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save user {Email} after Auth0 provisioning {Auth0Id}", dto.Email, auth0Id);
            return new Error(ErrorCode.InternalError, "Failed to create user in database");
        }

        return new CreateUserResponse(newUser.Id, ticket, false);
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
