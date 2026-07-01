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
        if (validation) return new Error("INVALID_OPERATION", "email or username taken");

        var branchIds = dto.BranchRoles.Select(br => br.BranchId).Distinct().ToList();
        var roleIds = dto.BranchRoles.Select(br => br.RoleId).Distinct().ToList();

        var branchesResult = await branchService.GetBranchesByIds(branchIds);
        var rolesResult = await ValidateRoles(roleIds);

        if (!branchesResult.IsSuccess) return new Error("NOT_FOUND", branchesResult.Error?.Message ?? "");
        if (!rolesResult.IsSuccess) return new Error("NOT_FOUND", rolesResult.Error?.Message ?? "");

        var userId = Guid.NewGuid();

        var newUser = User.CreateStandard(dto.Email, dto.Username);
        newUser.UserBranchRoles = dto.BranchRoles.Select(br => new UserBranchRole
        {
            BranchId = br.BranchId,
            RoleId = br.RoleId,
        }).ToList();

        context.Users.Add(newUser);

        var verificationCode = EmailVerificationCode.CreateForAccountSetup(userId, dto.Email);
        context.EmailVerificationCodes.Add(verificationCode);

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
            return new Error("NOT_FOUND", $"roles not found, missing: {string.Join(", ", missingRoleIds)}");
        }

        return true;
    }
}