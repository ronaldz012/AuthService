using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using module.Auth.Domain;
using module.Auth.dtos.Features;

namespace module.Auth.Features.Features;

public class CreateFeature(AuthDbContext dbContext)
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
            Module = dto.Module
        };
        dbContext.Features.Add(feature);
        await dbContext.SaveChangesAsync();
        return feature.Id;
    }
}
