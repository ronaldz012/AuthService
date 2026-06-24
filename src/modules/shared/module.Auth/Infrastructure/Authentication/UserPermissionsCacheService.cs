using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using module.Auth.dtos.permissions;
using module.Auth.dtos.Roles;
using module.Auth.interfaces;

namespace module.Auth.cache;

public class UserPermissionsCacheService(
    IMemoryCache cache,
    AuthDbContext context,
    IBranchService branchService,
    IFeatureService featureService) : IUserPermissionsCacheService
{
    private static readonly MemoryCacheEntryOptions Opts =
        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    private static string Key(Guid userId) => $"user_branches:{userId}";

    public async Task<List<PermissionsDto>> GetAsync( Guid userId, bool isAdmin)
    {
        if (cache.TryGetValue(Key(userId), out List<PermissionsDto>? cached) && cached is not null)
            return cached;

        var userBranchRoles = await context.UserBranchRoles
            .Include(ubr => ubr.Branch)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RoleFeaturePermissions)
            .Where(u => u.UserId == userId).ToListAsync();

        var userBranches = userBranchRoles
            .Select(ubr => ubr.Branch)
            .Distinct()
            .ToList();

        if (!userBranches.Any()) return [];

        List<PermissionsDto> branches;

        if (isAdmin)
        {
            var allFeatures = await featureService.GetAllFeaturesAsync();

            branches = userBranches.Select(branch => new PermissionsDto
            {
                BranchId   = branch.Id,
                BranchName = branch.Name,
                Roles      = [],
                Features   = allFeatures.Select(f => new FeaturePermissionsDeductedDtos
                {
                    Id         = f.Id,
                    Name       = f.Name,
                    Route      = f.Route,
                    ModuleName = f.ModuleName,
                    CanRead    = true,
                    CanCreate  = true,
                    CanUpdate  = true,
                    CanDelete  = true,
                }).ToList()
            }).ToList();
        }
        else
        {
            var featureIds = userBranchRoles
                .SelectMany(ubr => ubr.Role.RoleFeaturePermissions)
                .Select(rmp => rmp.FeatureId)
                .Distinct();

            var features = await featureService.GetFeaturesByIdsAsync(featureIds);
            var featureMap = features.ToDictionary(f => f.Id);
            branches = userBranchRoles.Select(ubr => new PermissionsDto
            {
                BranchId = ubr.Branch.Id,
                BranchName = ubr.Branch.Name,
                Roles = [
                    new RoleDto 
                    { 
                        Id = ubr.Role.Id, 
                        Name = ubr.Role.Name 
                    }
                ],
                Features = ubr.Role.RoleFeaturePermissions
                    .Select(rfp => 
                    {
                        if (!featureMap.TryGetValue(rfp.FeatureId, out var fInfo)) 
                            return null!;

                        return new FeaturePermissionsDeductedDto
                        {
                            Id = fInfo.Id,
                            CanRead = rfp.CanRead,
                            CanCreate = rfp.CanCreate,
                            CanUpdate = rfp.CanUpdate,
                            CanDelete = rfp.CanDelete
                        };
                    })
                    .ToList()
            }).ToList();
        }

        cache.Set(Key(userId), branches, Opts);
        return branches;
    }

    public void Invalidate(Guid userId) =>
        cache.Remove(Key(userId));

    public void Set(Guid userId, List<PermissionsDto> branches) =>
        cache.Set(Key(userId), branches, Opts);
}


