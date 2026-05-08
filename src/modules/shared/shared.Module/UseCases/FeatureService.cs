using Microsoft.EntityFrameworkCore;
using shared.Contracts.dtos;
using shared.Contracts.interfaces;
using shared.Module.Data;

namespace shared.Module.UseCases;

// Common/Database/UseCases/FeatureService.cs
public class FeatureService(SharedDbContext db) : IFeatureService
{
    public async Task<List<FeatureWithModuleDto>> GetFeaturesByIdsAsync(IEnumerable<int> featureIds)
    {
        var ids = featureIds.ToList();
        if (!ids.Any()) return [];

        return await db.Features
            .Where(f => ids.Contains(f.Id))
            .Include(f => f.Module)
            .Select(f => new FeatureWithModuleDto
            {
                Id            = f.Id,
                Name          = f.Name,
                Route         = f.Route,
                Icon          = f.Icon,
                ModuleId      = f.Module.Id,
                ModuleName    = f.Module.Name,
                ModuleRoute   = f.Module.Route,
                ModuleIcon    = f.Module.Icon,
                ModuleIsEnabled = f.Module.IsEnabled,
            })
            .ToListAsync();
    }
}