using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Roles.GetRoles;

public class GetRoles(IAuthDbContext context)
{
    public async Task<Result<List<RoleListItemDto>>> Execute()
    {
        return await context.Roles
            .Select(x => new RoleListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
            })
            .ToListAsync();
    }
}
