
using Auth.Data.Persistence;
using Auth.Dtos.Modules;

namespace Auth.UseCases.Menus;

public class UpdateMenu(AuthDbContext dbContext)
{
    public async Task<CreateMenuDto> Execute(UpdateMenuDto dto)
    {
        return null!;
    }

}
