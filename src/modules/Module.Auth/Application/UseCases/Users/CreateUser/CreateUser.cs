using Common.Contracts.authentication;
using Common.Contracts.branches;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.CreateUser;

public class CreateUser(IAuthDbContext context, IBranchService branchService, ITenantContext tenantContext)
{
    public async Task<Result<string>> Execute(CreateUserRequest dto)
    {
        var validation = await context.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username);
        if (validation) return CreateUserErrors.EmailOrUsernameTaken;

        var branchIds = dto.BranchRoles.Select(br => br.BranchId).Distinct().ToList();
        var roleIds = dto.BranchRoles.Select(br => br.RoleId).Distinct().ToList();

        var branchesResult = await branchService.GetBranchesByIds(branchIds);
        var rolesResult = await ValidateRoles(roleIds);

        if (!branchesResult.IsSuccess) return CreateUserErrors.BranchesNotFound;
        if (!rolesResult.IsSuccess) return CreateUserErrors.RolesNotFound;


        var newUser = User.CreateStandard(dto.Email, dto.Username);
        newUser.UserBranchRoles = dto.BranchRoles.Select(br => new UserBranchRole
        {
            BranchId = br.BranchId,
            RoleId = br.RoleId,
        }).ToList();
        var verificationCode = EmailVerificationCode.CreateForAccountSetup(dto.Email);
        newUser.EmailVerificationCodes.Add(verificationCode);
        context.Users.Add(newUser);


        await context.SaveChangesAsync();

        return verificationCode.Code;
    }

    private async Task<Result<bool>> ValidateRoles(List<Guid> roleIds)
    {
        var foundRolesIds = await context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync();

        var missingRoleIds = roleIds.Except(foundRolesIds).ToList();

        if (missingRoleIds.Any())
        {
            return CreateUserErrors.MissingRoles;
        }

        return true;
    }
}