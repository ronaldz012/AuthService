using Auth.Contracts.Dtos.Users;
using Auth.Contracts.Interfaces;
using Auth.Data.Persistence;
using Auth.UseCases.Autentication.functions;
using Branches.Contracts;
using Branches.module.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Common.Services;
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

        var featureIds = user.UserBranchRoles
            .SelectMany(ubr => ubr.Role.RoleFeaturePermissions)
            .Select(rmp => rmp.FeatureId)
            .Distinct();

        var features = await featureService.GetFeaturesByIdsAsync(featureIds);
        var featureMap = features.ToDictionary(f => f.Id);

        var branchResult = await UserMappingUtils.BuildBranchAccessByModule(user, branchService, featureMap);
        if (!branchResult.IsSuccess)
            return new Error("NOT_FOUND", branchResult.Error.Message);

        return new SuccessLoginDto
        {
            Status       = user.Status.ToString(),
            AuthProvider = user.AuthProvider.ToString(),
            Branches     = branchResult.Value,
            User         = mapper.Map<UserDetailsDto>(user)
        };
    }
}