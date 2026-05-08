using System;
using Auth.Contracts.Dtos.Roles;
using Auth.Data;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Common.Result;

namespace Auth.UseCases.Roles;

public class GetRole(AuthDbContext dbContext )
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
                CanUpdate = x.CanUpdate,
                CanDelete = x.CanDelete,
                CanCreate = x.CanCreate,
                CanRead = x.CanRead,
            }).ToList()
        };
    }
}
