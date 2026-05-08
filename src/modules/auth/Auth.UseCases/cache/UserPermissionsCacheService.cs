using System;
using Auth.Contracts.Dtos.permissions;
using Auth.Contracts.Dtos.Users;
using Auth.Contracts.Interfaces;
using Auth.Data;
using Auth.Data.Entities;
using Auth.UseCases.Autentication;
using Auth.UseCases.Autentication.functions;
using Branches.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using shared.Contracts.interfaces;

namespace Auth.UseCases.cache;

public class UserPermissionsCacheService(IMemoryCache cache, AuthDbContext context, IBranchService branchService, IFeatureService featureService) : IUserPermissionsCacheService
{
    private static readonly MemoryCacheEntryOptions Opts =
        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    private static string Key(Guid userId) => $"user_branches:{userId}";

    public async Task<List<PermissionsDto>> GetAsync(Guid userId)
    {
        if (cache.TryGetValue(Key(userId), out List<PermissionsDto>? cached) && cached is not null)
            return cached;
        var user = await context.Users
            .AsSplitQuery()
            .Include(u => u.UserBranchRoles.Where(ur => ur.DeletedAt == null))
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RoleFeaturePermissions)
                    .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return [];
        var featureIds = user.UserBranchRoles
            .SelectMany(ubr => ubr.Role.RoleFeaturePermissions)
            .Select(rmp => rmp.FeatureId)
            .Distinct();

        var features = await featureService.GetFeaturesByIdsAsync(featureIds);
        var featureMap = features.ToDictionary(f => f.Id);
        var branchResult = await UserMappingUtils.BuildBranchAccess(user, branchService,featureMap);
        if (!branchResult.IsSuccess) return [];

        cache.Set(Key(userId), branchResult.Value, Opts);
        return branchResult.Value;
    }

    public void Invalidate(Guid userId) =>
        cache.Remove(Key(userId));

    public void Set(Guid userId, List<PermissionsDto> branches)
    {
        cache.Set(Key(userId), branches, Opts);
    }
}
