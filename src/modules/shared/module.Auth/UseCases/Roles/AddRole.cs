using Common.Utilities;
using module.Auth.Domain;
using module.Auth.dtos.Roles;

namespace module.Auth.Features.Roles;

public class AddRole(AuthDbContext dbContext )
{
    public async Task<Result<Guid>> Execute(CreateRoleDto dto)
    {
        var role = new Role()
        {
            Name = dto.Name,
            Description = dto.Description,
            RoleFeaturePermissions = dto.RoleModulePermissions.Select( f =>new RoleFeaturePermission()
            {
                FeatureId = f.FeatureId,
                CanCreate =  f.CanCreate,
                CanUpdate = f.CanUpdate,
                CanDelete = f.CanDelete,
                CanRead =  f.CanRead
            }).ToList()
        };
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();
        return role.Id; 
    }
}
