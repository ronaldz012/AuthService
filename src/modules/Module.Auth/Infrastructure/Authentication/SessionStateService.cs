using Common.Contracts.authentication.dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Authentication;

public class SessionStateService(
    IMemoryCache cache,
    IAuthDbContext context) : ISessionStateService
{
    private static readonly MemoryCacheEntryOptions CacheOpts =
        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    private static string Key(Guid userId) => $"session_state:{userId}";

    public async Task<SessionStateDto> GetOrBuildAsync(Guid userId, Guid tenantId, bool isAdmin)
    {
        if (cache.TryGetValue(Key(userId), out SessionStateDto? cached) && cached is not null)
            return cached;

        var session = await BuildAsync(userId, tenantId, isAdmin);
        cache.Set(Key(userId), session, CacheOpts);
        return session;
    }

    public void Invalidate(Guid userId) => cache.Remove(Key(userId));

    private async Task<SessionStateDto> BuildAsync(Guid userId, Guid tenantId, bool isAdmin)
    {
        var user = await context.Users
            .IgnoreQueryFilters()
            .Where(u => u.Id == userId)
            .Select(u => new UserDetailResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                UserType = (int)u.Type,
                FirstName = u.FirstName,
                LastName = u.LastName,
            })
            .FirstAsync();

        var tenantInfo = await context.Tenants.IgnoreQueryFilters()
            .Include(t => t.Plan)
            .Include(t => t.Branches)
            .FirstAsync(t => t.Id == tenantId);

        List<PermissionsByModuleDto> branches;

        if (isAdmin)
        {
            var planFeatures = await context.Features
                .Where(f => tenantInfo.Plan.AllowedFeatureKeys.Contains(f.Key))
                .ToListAsync();

            branches = tenantInfo.Branches
                .Where(b => b.IsActive)
                .Select(branch => new PermissionsByModuleDto
                {
                    BranchId = branch.Id,
                    BranchName = branch.Name,
                    Role = "Admin",
                    Modules = planFeatures
                        .GroupBy(f => f.Module.ToString())
                        .Select(moduleGroup => new PermissiónByModuleDto
                        {
                            Name = moduleGroup.Key,
                            Route = $"/{moduleGroup.Key.ToLower()}",
                            Features = moduleGroup.Select(f => new FeaturePermissionByModuleDto
                            {
                                key = f.Key,
                                DisplayName = f.DisplayName,
                                route = f.Route,
                                icon = f.Icon,
                                Permission = ["*"],
                                IsMenu = f.IsMenu,
                            }).ToList()
                        }).ToList()
                }).ToList();
        }
        else
        {
            var rawPermissions = await context.UserBranchRoles
                .AsSplitQuery()
                .Where(ubr => ubr.UserId == userId)
                .Select(ubr => new
                {
                    ubr.BranchId,
                    BranchName = ubr.Branch.Name,
                    RoleName = ubr.Role.Name,
                    Features = ubr.Role.RoleFeaturePermissions.Select(rfp => new
                    {
                        rfp.FeatureKey,
                        rfp.Feature.IsMenu,
                        rfp.Feature.DisplayName,
                        rfp.Feature.Module,
                        rfp.Feature.Route,
                        rfp.Feature.Icon,
                        rfp.Permissions
                    }).ToList()
                })
                .ToListAsync();

            branches = rawPermissions.Select(b => new PermissionsByModuleDto
            {
                BranchId = b.BranchId,
                BranchName = b.BranchName,
                Role = b.RoleName,
                Modules = b.Features
                    .GroupBy(f => f.Module.ToString())
                    .Select(moduleGroup => new PermissiónByModuleDto
                    {
                        Name = moduleGroup.Key,
                        Route = $"/{moduleGroup.Key.ToLower()}",
                        Features = moduleGroup.Select(f => new FeaturePermissionByModuleDto
                        {
                            key = f.FeatureKey,
                            DisplayName = f.DisplayName,
                            route = f.Route,
                            icon = f.Icon,
                            Permission = f.Permissions,
                            IsMenu = f.IsMenu,
                        }).ToList()
                    }).ToList()
            }).ToList();
        }

        var activeUsers = await context.Users.IgnoreQueryFilters()
            .CountAsync(u => u.TenantId == tenantId && u.IsActive);

        var activeBranches = await context.Branches.IgnoreQueryFilters()
            .CountAsync(b => b.TenantId == tenantId && b.IsActive);

        var tenantPlan = new TenantPlanUsageDto(
            tenantInfo.Plan.Name,
            tenantInfo.Plan.AllowedFeatureKeys,
            tenantInfo.Plan.MaxUsers,
            activeUsers,
            tenantInfo.Plan.MaxBranches,
            activeBranches
        );

        return new SessionStateDto(user, branches, tenantPlan);
    }
}
