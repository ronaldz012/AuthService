using Auth.Contracts.Dtos.permissions;
using Auth.Contracts.Dtos.Roles;
using Auth.Contracts.Dtos.Users;
using Auth.Data.Entities;
using Branches.Contracts;
using Common.Result;
using shared.Contracts.dtos;

namespace Auth.UseCases.Autentication.functions;

public static class UserMappingUtils
{
    // ─── Helpers privados ────────────────────────────────────────────────────

    private static List<FeaturePermissionsDeductedDto> CalculateFeaturePermissions(
        List<UserBranchRole> branchRoles,
        Dictionary<int, FeatureWithModuleDto> featureMap)
    {
        return branchRoles
            .SelectMany(ubr => ubr.Role.RoleFeaturePermissions)
            .GroupBy(rmp => rmp.FeatureId)
            .Where(g => featureMap.ContainsKey(g.Key))
            .Select(g =>
            {
                var feature = featureMap[g.Key];
                return new FeaturePermissionsDeductedDto
                {
                    Id        = feature.Id,
                    Name      = feature.Name,
                    Route     = feature.Route,
                    ModuleId  = feature.ModuleId,
                    ModuleName = feature.ModuleName,
                    CanRead   = g.Any(rmp => rmp.CanRead),
                    CanCreate = g.Any(rmp => rmp.CanCreate),
                    CanUpdate = g.Any(rmp => rmp.CanUpdate),
                    CanDelete = g.Any(rmp => rmp.CanDelete),
                };
            }).ToList();
    }

    private static List<PermissiónByModuleDto> CalculateFeatureByModule(
        List<UserBranchRole> userBranchRoles,
        Dictionary<int, FeatureWithModuleDto> featureMap)
    {
        return userBranchRoles
            .SelectMany(ubr => ubr.Role.RoleFeaturePermissions)
            .Where(rfp => featureMap.ContainsKey(rfp.FeatureId))
            .GroupBy(rfp => featureMap[rfp.FeatureId].ModuleId)
            .Select(g =>
            {
                var module = featureMap[g.First().FeatureId];
                return new PermissiónByModuleDto
                {
                    Id          = module.ModuleId,
                    Name        = module.ModuleName,
                    Description = module.ModuleDescription,
                    Route       = module.ModuleRoute,
                    Icon        = module.ModuleIcon,
                    Features    = g
                        .GroupBy(rfp => rfp.FeatureId)
                        .Select(fg =>
                        {
                            var feature = featureMap[fg.Key];
                            return new FeaturePermissionByModuleDto
                            {
                                Id        = feature.Id,
                                Name      = feature.Name,
                                Route     = feature.Route,
                                Icon      = feature.Icon,
                                CanRead   = fg.Any(rfp => rfp.CanRead),
                                CanCreate = fg.Any(rfp => rfp.CanCreate),
                                CanUpdate = fg.Any(rfp => rfp.CanUpdate),
                                CanDelete = fg.Any(rfp => rfp.CanDelete),
                            };
                        }).ToList()
                };
            }).ToList();
    }

    // ─── Métodos públicos ─────────────────────────────────────────────────────

    public static async Task<Result<List<PermissionsDto>>> BuildBranchAccess(
        User user,
        IBranchService branchService,
        Dictionary<int, FeatureWithModuleDto> featureMap)
    {
        var branchIds = user.UserBranchRoles
            .Select(ubr => ubr.BranchId)
            .Distinct()
            .ToList();

        var branchesResult = await branchService.GetBranchesByIds(branchIds);
        if (!branchesResult.IsSuccess)
            return new Error("NOT_FOUND", branchesResult.Error.Message);

        var branchesById = branchesResult.Value.ToDictionary(b => b.Id);

        return user.UserBranchRoles
            .GroupBy(ubr => ubr.BranchId)
            .Select(g =>
            {
                var branch = branchesById[g.Key];
                return new PermissionsDto
                {
                    BranchId   = branch.Id,
                    BranchName = branch.Name,
                    Roles      = g.Select(ubr => new RoleDto
                    {
                        Id   = ubr.Role.Id,
                        Name = ubr.Role.Name
                    }).ToList(),
                    Features = CalculateFeaturePermissions(g.ToList(), featureMap)
                };
            }).ToList();
    }

    public static async Task<Result<List<PermissionsByModuleDto>>> BuildBranchAccessByModule(
        User user,
        IBranchService branchService,
        Dictionary<int, FeatureWithModuleDto> featureMap)
    {
        var branchIds = user.UserBranchRoles
            .Select(ubr => ubr.BranchId)
            .Distinct()
            .ToList();

        var branchesResult = await branchService.GetBranchesByIds(branchIds);
        if (!branchesResult.IsSuccess)
            return new Error("NOT_FOUND", branchesResult.Error.Message);

        var branchesById = branchesResult.Value.ToDictionary(b => b.Id);

        return user.UserBranchRoles
            .GroupBy(ubr => ubr.BranchId)
            .Select(g =>
            {
                var branch = branchesById[g.Key];
                return new PermissionsByModuleDto
                {
                    BranchId   = branch.Id,
                    BranchName = branch.Name,
                    Roles      = g.Select(ubr => new RoleDto
                    {
                        Id   = ubr.Role.Id,
                        Name = ubr.Role.Name
                    }).ToList(),
                    Modules = CalculateFeatureByModule(g.ToList(), featureMap)
                };
            }).ToList();
    }
}