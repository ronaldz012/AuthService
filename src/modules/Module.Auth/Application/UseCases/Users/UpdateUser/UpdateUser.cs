using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.UpdateUser;

public class UpdateUser(IAuthDbContext context)
{
    public async Task<Result<UpdateUserResponse>> Execute(ActorContext ctx, Guid id, UpdateUserRequest dto)
    {
        var user = await context.Users
            .Include(u => u.UserBranchRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return UpdateUserErrors.UserNotFound;

        if (dto.BranchRoles is not null)
        {
            var branchIds = dto.BranchRoles.Select(br => br.BranchId).Distinct().ToList();
            var roleIds = dto.BranchRoles.Select(br => br.RoleId).Distinct().ToList();

            var foundBranchIds = await context.Branches
                .Where(b => branchIds.Contains(b.Id))
                .Select(b => b.Id)
                .ToListAsync();

            if (foundBranchIds.Count != branchIds.Count)
                return UpdateUserErrors.BranchesNotFound;

            var foundRoleIds = await context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync();

            if (foundRoleIds.Count != roleIds.Count)
                return UpdateUserErrors.RolesNotFound;

            var existing = user.UserBranchRoles.ToList();

            var toRemove = existing
                .Where(e => !dto.BranchRoles.Any(br => br.BranchId == e.BranchId && br.RoleId == e.RoleId))
                .ToList();

            var toAdd = dto.BranchRoles
                .Where(br => !existing.Any(e => e.BranchId == br.BranchId && e.RoleId == br.RoleId))
                .Select(br => UserBranchRole.Create(user.Id, br.BranchId, br.RoleId, ctx.UserId, ctx.FullName))
                .ToList();

            foreach (var item in toRemove)
                user.UserBranchRoles.Remove(item);

            foreach (var item in toAdd)
                user.UserBranchRoles.Add(item);
        }

        user.UpdateProfile(dto.FirstName, dto.LastName, dto.Ci, dto.Nationality, dto.BirthDate, ctx.UserId, ctx.FullName);

        await context.SaveChangesAsync();

        return new UpdateUserResponse(user.Id);
    }
}
