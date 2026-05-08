
using Common.Result;
using shared.Contracts.dtos.Modules;
using shared.Module.Data;
using shared.Module.Entities;

namespace shared.Module.UseCases.Modules;

public class CreateModuleUseCase(SharedDbContext context)
{
    public async Task<Result<ModuleDto>> Execute(CreateModuleDto dto)
    {
        var newModule = new Entities.Module
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            Route = dto.Route,
            Features = dto.Features.Select(f => new Feature()
            {
                Name = f.Name,
                Icon = f.Icon,
                Description =  f.Description,
                Route = f.Route,
                
            }).ToList()
        };
        context.Add(newModule);
        await context.SaveChangesAsync();
        return new ModuleDto
        {
            Id = newModule.Id,
            Name = newModule.Name,
            Description = newModule.Description,
            Icon = newModule.Icon,
            IsEnabled = newModule.IsEnabled,
        };
    }
}