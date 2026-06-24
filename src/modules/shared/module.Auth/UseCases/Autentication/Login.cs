using Common.Contracts.authentication;
using Common.Contracts.branches;
using Common.permissions;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using module.Auth.Domain;
using module.Auth.dtos.Users;
using module.Auth.Features.branches;
using module.Auth.interfaces;

namespace module.Auth.Features.Autentication;

public class Login(
    AuthDbContext dbContext,
    ITokenGenerator tokenGenerator,
    IBranchService branchService,
    ITenantContext tenantContext,
    IFeatureService featureService,
    IUserPermissionsCacheService permissionsCache) // Inyectado el servicio de caché
{
    public async Task<Result<SuccessLoginDto>> Execute(LoginDto request)
    {
        // 1. Query del usuario
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return new Error("VALIDATION_ERROR", "Correo electrónico o contraseña incorrectos.");
        if (!ValidatePassword.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return new Error("VALIDATION_ERROR", "Correo electrónico o contraseña incorrectos.");

        if (user.Status == UserStatus.PendingVerification)
        {
            return new SuccessLoginDto
            {
                Status = user.Status.ToString(),
                User = new UserDetailsDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                }
            };
        }

        var isAdmin = user.IsAdmin;
        Dictionary<Guid, BranchResponse> branchesById;

        var userBranchesCache = await permissionsCache.GetAsync(user.Id, isAdmin);

        List<PermissionsByModuleDto> branches = userBranchesCache.Select(b => new PermissionsByModuleDto
        {
            BranchId = b.BranchId,
            BranchName = b.BranchName,
            Roles = b.Roles,
            Modules = b.Features
                .GroupBy(f => new { f.ModuleName })
                .Select(gModule => new PermissiónByModuleDto
                {
                    Name = gModule.Key.ModuleName,
                    Features = gModule.Select(f => new FeaturePermissionByModuleDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Route = f.Route,
                        CanCreate = f.CanCreate,
                        CanRead = f.CanRead,
                        CanUpdate = f.CanUpdate,
                        CanDelete = f.CanDelete
                    }).ToList()
                }).ToList()
        }).ToList();

        // 4. Generación de tokens
        var accessToken = tokenGenerator.GenerateAccessToken(user.Id, tenantContext.TenantId!.Value,
            tenantContext.Schema ?? "", tenantContext.DatabaseName ?? "", user.IsAdmin);
        var refreshToken = tokenGenerator.GenerateRefreshToken();

        return new SuccessLoginDto
        {
            Status = user.Status.ToString(),
            AuthProvider = user.AuthProvider.ToString(),
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = tokenGenerator.GetAccessTokenExpirationMinutes() * 60,
            Branches = branches,
            User = new UserDetailsDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            }
        };
    }
}
