using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Infrastructure.Authentication;

public class UserPermissionsCacheService(
    IMemoryCache cache,
    IAuthDbContext context,
    ILogger<UserPermissionsCacheService> logger) : IUserPermissionsCacheService
{
    private static readonly MemoryCacheEntryOptions Opts =
        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    private static string Key(Guid userId) => $"user_permissions:{userId}";

    public async Task<List<PermissionsDto>> GetAsync(Guid userId, Guid tenantId, bool isAdmin)
    {
        // if (cache.TryGetValue(Key(userId), out List<PermissionsDto>? cached) && cached is not null)
        //     return cached;

        List<PermissionsDto> branches;

        if (isAdmin)
        {

            var tenantData = await context.Tenants.IgnoreQueryFilters()
                .Include(t => t.Plan)
                .Include(t => t.Branches)
                .FirstOrDefaultAsync(t => t.Id == tenantId);


            var planFeatures = await context.Features
                .Where(f => tenantData!.Plan.AllowedFeatureKeys.Contains(f.Key))
                .ToListAsync();

            branches = tenantData!.Branches.Select(branch => new PermissionsDto
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                RoleName = "Admin", 
                Features = planFeatures.Select(f => new FeaturePermissionsDto
                {
                    Key = f.Key,
                    DisplayName = f.DisplayName,
                    Route = f.Route,
                    Icon = f.Icon,
                    IsMenu = f.IsMenu,
                    ModuleName = f.Module.ToString(),
                    Permissions = ["*"] 
                }).ToList()
            }).ToList();
        }
        else
        {
            branches = await context.UserBranchRoles
                .AsSplitQuery()
                .Where(ubr => ubr.UserId == userId)
                .Select(ubr => new PermissionsDto
                {
                    BranchId = ubr.BranchId,
                    BranchName = ubr.Branch.Name,
                    RoleName = ubr.Role.Name,
                    Features = ubr.Role.RoleFeaturePermissions.Select(rfp => new FeaturePermissionsDto
                    {
                        Key = rfp.FeatureKey,
                        IsMenu = rfp.Feature.IsMenu,
                        DisplayName =  rfp.Feature.DisplayName,
                        ModuleName = rfp.Feature.Module.ToString(),
                        Permissions = rfp.Permissions
                    }).ToList()
                }).ToListAsync();
        }

        cache.Set(Key(userId), branches, Opts);
        return branches;
    }

    public void Invalidate(Guid userId) => cache.Remove(Key(userId));

    public void Set(Guid userId, List<PermissionsDto> branches) => cache.Set(Key(userId), branches, Opts);
}