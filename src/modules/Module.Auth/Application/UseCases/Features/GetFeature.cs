using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Features;

public class GetFeature(IAuthDbContext dbContext )
{
    public async Task<Result<FeatureDetailsDto?>> Execute(string key)
    {
        var feature = await dbContext.Features.Where(m => m.Key == key).FirstOrDefaultAsync();
        if (feature == null)
            return new Error("NOT_FOUND", "Module not found");

        return new FeatureDetailsDto()
        {
            Name = feature.Key,
            Description = feature.Description,
            Icon = feature.Icon,
        };
    }
}
