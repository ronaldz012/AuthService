using System;
using Auth.Data.Persistence;
using Auth.Dtos.Modules;

namespace Auth.UseCases.Modules;

public class AddModule(AuthDbContext dbContext)
{

    public async Task<int> Execute(CreateModuleDto dto)
    {
        var module = new Data.Entities.Module
        {
            Name = dto.Name,
            Description = dto.Description
        };
        dbContext.Modules.Add(module);
        await dbContext.SaveChangesAsync();
        return module.Id;
    }
}
