using System;
using Auth.Data.Entities;
using Auth.Data.Persistence;
using Auth.Dtos.Roles;
using MapsterMapper;
using Shared.Result;

namespace Auth.UseCases.Roles;

public class AddRole(AuthDbContext dbContext, IMapper mapper)
{
    public async Task<Result<int>> Execute(CreateRoleDto dto)
    {
        var role = mapper.Map<Role>(dto);
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();
        return role.Id; 
    }
}
