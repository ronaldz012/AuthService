using Common.Utilities;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Roles;

public class AddRole(IAuthDbContext dbContext )
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
                Permissions = f.Permissions
            }).ToList()
        };
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();
        return role.Id; 
    }
}
