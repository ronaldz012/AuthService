using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using module.Auth.dtos.Features;

namespace module.Auth.Features.Features;

public class GetFeature(AuthDbContext dbContext )
{
    public async Task<Result<FeatureDetailsDto?>> Execute(int id)
    {
        var feature = await dbContext.Features.Where(m => m.Id == id).FirstOrDefaultAsync();
        if (feature == null)
            return new Error("NOT_FOUND", "Module not found");

        return new FeatureDetailsDto()
        {
            Id = feature.Id,
            Name = feature.Name,
            Description = feature.Description,
            Icon = feature.Icon,
        };
    }
}
