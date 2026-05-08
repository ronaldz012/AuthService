using Auth.Contracts.Dtos.permissions;
using Auth.Contracts.Dtos.Roles;
using Auth.Contracts.Dtos.Users;
using Auth.Data.Entities;
using Branches.Contracts;
using Branches.Contracts.Dtos;
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
                    Id         = feature.Id,
                    Name       = feature.Name,
                    Route      = feature.Route,
                    ModuleId   = feature.ModuleId,
                    ModuleName = feature.ModuleName,
                    CanRead    = g.Any(rmp => rmp.CanRead),
                    CanCreate  = g.Any(rmp => rmp.CanCreate),
                    CanUpdate  = g.Any(rmp => rmp.CanUpdate),
                    CanDelete  = g.Any(rmp => rmp.CanDelete),
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

    // ← nuevo: admin con todo en true, agrupa igual que CalculateFeatureByModule
    private static List<PermissiónByModuleDto> CalculateAdminModules(
        List<FeatureWithModuleDto> allFeatures)
    {
        return allFeatures
            .GroupBy(f => f.ModuleId)
            .Select(g =>
            {
                var module = g.First();
                return new PermissiónByModuleDto
                {
                    Id          = module.ModuleId,
                    Name        = module.ModuleName,
                    Description = module.ModuleDescription,
                    Route       = module.ModuleRoute,
                    Icon        = module.ModuleIcon,
                    Features    = g.Select(f => new FeaturePermissionByModuleDto
                    {
                        Id        = f.Id,
                        Name      = f.Name,
                        Route     = f.Route,
                        Icon      = f.Icon,
                        CanRead   = true,
                        CanCreate = true,
                        CanUpdate = true,
                        CanDelete = true,
                    }).ToList()
                };
            }).ToList();
    }

    // ─── Métodos públicos ─────────────────────────────────────────────────────

    // branchesById viene resuelto desde afuera — Login se encarga de llamar branchService
    public static List<PermissionsDto> BuildBranchAccess(
        User user,
        Dictionary<Guid, BranchDto> branchesById,
        Dictionary<int, FeatureWithModuleDto> featureMap)
    {
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
                    Features   = CalculateFeaturePermissions(g.ToList(), featureMap)
                };
            }).ToList();
    }

    public static List<PermissionsByModuleDto> BuildBranchAccessByModule(
        User user,
        Dictionary<Guid, BranchDto> branchesById,
        Dictionary<int, FeatureWithModuleDto> featureMap)
    {
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
                    Modules    = CalculateFeatureByModule(g.ToList(), featureMap)
                };
            }).ToList();
    }

    public static List<PermissionsByModuleDto> BuildAdminBranchAccess(
        Dictionary<Guid, BranchDto> branchesById,
        List<FeatureWithModuleDto> allFeatures)
    {
        var adminModules = CalculateAdminModules(allFeatures);

        return branchesById.Values.Select(branch => new PermissionsByModuleDto
        {
            BranchId   = branch.Id,
            BranchName = branch.Name,
            Roles      = [], // admin no necesita roles en el frontend
            Modules    = adminModules
        }).ToList();
    }
}