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

public class Login(AuthDbContext dbContext, ITokenGenerator tokenGenerator, IMapper mapper, IBranchService branchService,ITenantContext tenantContext, IFeatureService featureService)
{
  public async Task<Result<SuccessLoginDto>> Execute(LoginDto request)
    {
        // 1. Query en AuthDbContext — ya sin Include hacia Feature/Module
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
        {
            return new SuccessLoginDto
            {
                Status = user.Status.ToString(),
                User   = mapper.Map<UserDetailsDto>(user)
            };
        }

        // 2. Recolectar todos los featureIds del usuario
        var featureIds = user.UserBranchRoles
            .SelectMany(ur => ur.Role.RoleFeaturePermissions)
            .Select(rmp => rmp.FeatureId)
            .Distinct();

        // 3. Query en SharedDbContext
        var features = await featureService.GetFeaturesByIdsAsync(featureIds);
        var featureMap = features.ToDictionary(f => f.Id);  // ← se pasa a los métodos
        var branchResult = await UserMappingUtils.BuildBranchAccessByModule(user, branchService, featureMap);
        if (!branchResult.IsSuccess)
            return new Error("NOT_FOUND", branchResult.Error.Message);

        var accessToken = tokenGenerator.GenerateAccessToken(user.Id, tenantContext.Schema ?? "");
        var refreshToken = tokenGenerator.GenerateRefreshToken();

        return new SuccessLoginDto
        {
            Status       = user.Status.ToString(),
            AuthProvider = user.AuthProvider.ToString(),
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn    = tokenGenerator.GetAccessTokenExpirationMinutes() * 60,
            User         = mapper.Map<UserDetailsDto>(user),
            Branches     = branchResult.Value
        };
    }
}