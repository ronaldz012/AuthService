using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Roles;

public class GetRole(IAuthDbContext dbContext )
{

    public async Task<Result<RoleDetailsDto>> Execute(Guid roleId)
    {
        var role = await dbContext.Roles.Where(r => r.Id == roleId)
            .Include(r => r.RoleFeaturePermissions)
            .FirstOrDefaultAsync();
        if (role == null)
            return new Error("NOT_FOUND", "Role not found");

        
        return new RoleDetailsDto()
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            FeaturePermissions = role.RoleFeaturePermissions.Select(x => new FeaturePermissionsDto()
            {
                FeatureId = x.FeatureId,
                Permissions = x.Permissions
            }).ToList()
        };
    }
}
