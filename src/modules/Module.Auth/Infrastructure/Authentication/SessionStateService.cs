using System.Collections.Concurrent;
using Common.Contracts.authentication.dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Authentication;

public class SessionStateService(
    IMemoryCache cache,
    IAuthDbContext context) : ISessionStateService
{
    private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> TenantCts = new();

    private static readonly MemoryCacheEntryOptions CacheOpts =
        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    private static string Key(Guid userId) => $"session_state:{userId}";

    public async Task<SessionStateDto> GetOrBuildAsync(Guid userId, Guid tenantId, UserType userType)
    {
        if (cache.TryGetValue(Key(userId), out SessionStateDto? cached) && cached is not null)
            return cached;

        var session = await BuildAsync(userId, tenantId, userType);

        var cts = TenantCts.GetOrAdd(tenantId, _ => new CancellationTokenSource());
        var opts = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .AddExpirationToken(new CancellationChangeToken(cts.Token));

        cache.Set(Key(userId), session, opts);
        return session;
    }

    public void Invalidate(Guid userId) => cache.Remove(Key(userId));

    public void InvalidateTenant(Guid tenantId)
    {
        if (TenantCts.TryGetValue(tenantId, out var oldCts))
        {
            oldCts.Cancel();
            oldCts.Dispose();
            TenantCts.TryUpdate(tenantId, new CancellationTokenSource(), oldCts);
        }
    }

    private async Task<SessionStateDto> BuildAsync(Guid userId, Guid tenantId, UserType userType)
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

        List<PermissionsByBranchDto> branches;

        if (userType is UserType.TenantAdmin or UserType.Owner)
        {
            var planFeatures = await context.Features
                .Where(f => tenantInfo.Plan.AllowedFeatureKeys.Contains(f.Key))
                .ToListAsync();

            branches = tenantInfo.Branches
                .Where(b => b.IsActive)
                .Select(branch => new PermissionsByBranchDto
                {
                    BranchId = branch.Id,
                    BranchName = branch.Name,
                    Role = "Admin",
                    Features = planFeatures
                        .Where(f => branch.AllowedFeatureKeys.Contains(f.Key))
                        .OrderBy(f => f.Module)
                        .Select(f => new SessionFeatureDto
                        {
                            Key = f.Key,
                            DisplayName = f.DisplayName,
                            Route = f.Route,
                            Icon = f.Icon,
                            Module = f.Module.ToString().ToLower(),
                            IsMenu = f.IsMenu,
                            Permissions = ["*"]
                        })
                        .ToList()
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
                    AllowedFeatureKeys = ubr.Branch.AllowedFeatureKeys,
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

            branches = rawPermissions.Select(b => new PermissionsByBranchDto
            {
                BranchId = b.BranchId,
                BranchName = b.BranchName,
                Role = b.RoleName,
                Features = b.Features
                    .Where(f => b.AllowedFeatureKeys.Contains(f.FeatureKey))
                    .OrderBy(f => f.Module)
                    .Select(f => new SessionFeatureDto
                    {
                        Key = f.FeatureKey,
                        DisplayName = f.DisplayName,
                        Route = f.Route,
                        Icon = f.Icon,
                        Module = f.Module.ToString().ToLower(),
                        IsMenu = f.IsMenu,
                        Permissions = f.Permissions
                    })
                    .ToList()
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
