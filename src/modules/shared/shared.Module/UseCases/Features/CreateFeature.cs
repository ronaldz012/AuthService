using Microsoft.EntityFrameworkCore;
using Common.Result;
using shared.Contracts.dtos.Features;
using shared.Module.Data;
using shared.Module.Entities;

namespace shared.Module.UseCases.Features;

public class CreateFeature(SharedDbContext dbContext)
{

    public async Task<Result<int>> Execute(CreateFeatureDto dto)
    {
        var exists = await dbContext.Features
            .AnyAsync(m => m.Name == dto.Name);
        if (exists) return new Error("DUPLICATE", "Ya existe un módulo con ese nombre");

        var feature = new Feature()
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            Route = dto.Route,
            ModuleId = dto.ModuleId,
        };
        dbContext.Features.Add(feature);
        await dbContext.SaveChangesAsync();
        return feature.Id;
    }
}
