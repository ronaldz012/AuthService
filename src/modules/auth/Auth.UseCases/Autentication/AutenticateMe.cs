using Auth.Contracts.Dtos.permissions;
using Auth.Contracts.Dtos.Users;
using Auth.Contracts.Interfaces;
using Auth.Data;
using Auth.UseCases.Autentication.functions;
using Branches.Contracts;
using Branches.Contracts.Dtos;
using Branches.module.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Common.Services;
using shared.Contracts.dtos;
using shared.Contracts.interfaces;

namespace Auth.UseCases.Autentication;

public class AutenticateMe(
    AuthDbContext context,
    ICurrentUser currentUser,
    IMapper mapper,
    IBranchService branchService,
    IFeatureService featureService) : IAuthenticateMe
{
    public async Task<Result<SuccessLoginDto>> Execute()
{
    var user = await context.Users
        .AsSplitQuery()
        .Include(u => u.UserBranchRoles.Where(ur => ur.DeletedAt == null))
            .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RoleFeaturePermissions)
        .FirstOrDefaultAsync(u => u.Id == currentUser.UserId);

    if (user == null)
        return new Error("NOT_FOUND", "Usuario no encontrado.");

    var isAdmin = user.IsAdmin;

    Dictionary<Guid, BranchDto> branchesById;

    if (isAdmin)
    {
        var allBranchesResult = await branchService.GetAllBranches();
        if (!allBranchesResult.IsSuccess)
            return new Error("NOT_FOUND", allBranchesResult.Error.Message);

        branchesById = allBranchesResult.Value.ToDictionary(b => b.Id);
    }
    else
    {
        var branchIds = user.UserBranchRoles
            .Select(ubr => ubr.BranchId)
            .Distinct()
            .ToList();

        var branchesResult = await branchService.GetBranchesByIds(branchIds);
        if (!branchesResult.IsSuccess)
            return new Error("NOT_FOUND", branchesResult.Error.Message);

        branchesById = branchesResult.Value.ToDictionary(b => b.Id);
    }

    List<PermissionsByModuleDto> branches;

    if (isAdmin)
    {
        var allFeatures = await featureService.GetAllFeaturesAsync();
        branches = UserMappingUtils.BuildAdminBranchAccess(branchesById, allFeatures);
    }
    else
    {
        var featureIds = user.UserBranchRoles
            .SelectMany(ubr => ubr.Role.RoleFeaturePermissions)
            .Select(rmp => rmp.FeatureId)
            .Distinct();

        var features = await featureService.GetFeaturesByIdsAsync(featureIds);
        var featureMap = features.ToDictionary(f => f.Id);
        branches = UserMappingUtils.BuildBranchAccessByModule(user, branchesById, featureMap);
    }

    return new SuccessLoginDto
    {
        Status       = user.Status.ToString(),
        AuthProvider = user.AuthProvider.ToString(),
        Branches     = branches,
        User         = mapper.Map<UserDetailsDto>(user)
    };
}
}
