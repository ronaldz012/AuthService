using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Features;

public class CreateFeature(IAuthDbContext dbContext)
{

    public async Task<Result<string>> Execute(CreateFeatureDto dto)
    {
        var exists = await dbContext.Features
            .AnyAsync(m => m.Key == dto.Name);
        if (exists) return new Error("DUPLICATE", "Ya existe un módulo con ese nombre");

        var feature = new Feature()
        {
            Key = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            Route = dto.Route,
            Module = dto.Module
        };
        dbContext.Features.Add(feature);
        await dbContext.SaveChangesAsync();
        return feature.Key;
    }
}
