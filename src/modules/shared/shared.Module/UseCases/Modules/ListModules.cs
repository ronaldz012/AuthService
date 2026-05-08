using Microsoft.EntityFrameworkCore;

using Common.Result;
using shared.Contracts.dtos.Modules;
using shared.Module.Data;

namespace shared.Module.UseCases.Modules;

public class ListModules(SharedDbContext context)
{
    public async Task<Result<List<ModuleDto>> >Execute()
    {
        return await context.Modules.Select(x => new ModuleDto()
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            IsEnabled = x.IsEnabled,
            Icon = x.Icon,
        }).ToListAsync();
    }
}