using Auth.Contracts.Dtos.permissions;
using Auth.Contracts.Dtos.Roles;
using Auth.Contracts.Dtos.Users;
using Auth.Data;
using Auth.Data.Entities;
using Auth.Infrastructure.Authentication;
using Auth.UseCases.Autentication.functions;
using Branches.Contracts;
using Branches.Contracts.Dtos;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Common.Data;
using Common.Result;
using shared.Contracts.interfaces;

namespace Auth.UseCases.Autentication;
public class Login(AuthDbContext dbContext, ITokenGenerator tokenGenerator, IMapper mapper, IBranchService branchService, ITenantContext tenantContext, IFeatureService featureService)
{
    public async Task<Result<SuccessLoginDto>> Execute(LoginDto request)
    {
        // 1. Query
        var user = await dbContext.Users
            .AsSplitQuery()
            .Include(u => u.UserBranchRoles.Where(ur => ur.DeletedAt == null))
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RoleFeaturePermissions)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return new Error("VALIDATION_ERROR", "Correo electrónico o contraseña incorrectos.");

        if (!ValidatePassword.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return new Error("VALIDATION_ERROR", "Correo electrónico o contraseña incorrectos.");

        if (user.Status == UserStatus.PendingVerification)
            return new SuccessLoginDto { Status = user.Status.ToString(), User = mapper.Map<UserDetailsDto>(user) };

        // 2. Cortocircuito admin
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
                .SelectMany(ur => ur.Role.RoleFeaturePermissions)
                .Select(rmp => rmp.FeatureId)
                .Distinct();

            var features = await featureService.GetFeaturesByIdsAsync(featureIds);
            var featureMap = features.ToDictionary(f => f.Id);
            branches = UserMappingUtils.BuildBranchAccessByModule(user, branchesById, featureMap);
        }
        var accessToken = tokenGenerator.GenerateAccessToken(user.Id, tenantContext.TenantId!.Value,tenantContext.Schema ?? "",tenantContext.DatabaseName ?? "", user.IsAdmin);
        var refreshToken = tokenGenerator.GenerateRefreshToken();

        return new SuccessLoginDto
        {
            Status       = user.Status.ToString(),
            AuthProvider = user.AuthProvider.ToString(),
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn    = tokenGenerator.GetAccessTokenExpirationMinutes() * 60,
            User         = mapper.Map<UserDetailsDto>(user),
            Branches     = branches
        };
    }
}