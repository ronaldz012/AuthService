using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.Common;
using Module.Auth.Application.UseCases.Users.GetAllUsers;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Autentication.Login;

public class Login(
    IAuthDbContext dbContext,
    ITenantContext tenantContext,
    IUserPermissionsCacheService permissionsCache,
    ITokenGenerator tokenGenerator) 
{
    public async Task<Result<SuccessLoginResponse>> Execute(LoginRequest request)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user == null)
            return new Error("VALIDATION_ERROR", "Correo electrónico o contraseña incorrectos.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new Error("VALIDATION_ERROR", "Correo electrónico o contraseña incorrectos.");

        if (user.Status == UserStatus.PendingVerification)
        {
            return new SuccessLoginResponse
            {
                Status = user.Status.ToString(),
                User = new UserDetailResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                }
            };
        }

        var isAdmin = user.Type is UserType.TenantAdmin or UserType.Owner;
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
                        key = f.Key,
                        Name = f.ModuleName,
                        Permission = f.Permissions
                    }).ToList()
                }).ToList()
        }).ToList();

        // 4. Generación de tokens usando los métodos privados
        var expirationMinutes = tokenGenerator.GetExpirationMinutes();
        var accessToken = tokenGenerator.GenerateAccessToken(
            user.Id, 
            tenantContext.TenantId!.Value,
            tenantContext.Schema ?? "", 
            isAdmin); 
            
        var refreshToken =tokenGenerator.GenerateRefreshToken();

        return new SuccessLoginResponse
        {
            Status = user.Status.ToString(),
            AuthProvider = user.AuthProvider.ToString(),
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expirationMinutes * 60,
            Branches = branches,
            User = new UserDetailResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            }
        };
    }
    

}