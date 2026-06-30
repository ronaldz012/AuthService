using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.Common;
using Module.Auth.Application.UseCases.Users.GetAllUsers;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Autentication.Login;

public class Login(
    IAuthDbContext dbContext,
    ITenantContext tenantContext,
    IUserPermissionsCacheService permissionsCache,
    ITokenGenerator tokenGenerator,
    ILogger<Login> logger) 
{
    public async Task<Result<SuccessLoginResponse>> Execute(LoginRequest request)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

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
        
        tenantContext.TenantId = user.TenantId;
        
        List<PermissionsDto> flatPermissions = await permissionsCache.GetAsync(user.Id, user.TenantId, isAdmin);
        
        var branches = new List<PermissionsByModuleDto>();

        Console.WriteLine("############: " + flatPermissions.First().BranchName);
        var branchesResponse = flatPermissions.Select(b => new PermissionsByModuleDto
        {
            BranchId = b.BranchId,
            BranchName = b.BranchName,
            Role = b.RoleName,
    
            Modules = b.Features
                .GroupBy(f => f.ModuleName) 
                .Select(moduleGroup => new PermissiónByModuleDto
                {
                    Name = moduleGroup.Key.ToString(),
                    Route = $"/{moduleGroup.Key.ToString().ToLower()}",
                    Features = moduleGroup.Select(f => new FeaturePermissionByModuleDto
                    {
                        key = f.Key,
                        DisplayName = f.DisplayName,
                        route = f.Route,
                        icon = f.Icon,
                        Permission = f.Permissions, 
                        IsMenu = f.IsMenu 
                    }).ToList()
                }).ToList()
        }).ToList();

        var expirationMinutes = tokenGenerator.GetExpirationMinutes();
        var accessToken = tokenGenerator.GenerateAccessToken(
            user.Id,
            user.TenantId,
            tenantContext.Schema ?? "",
            isAdmin);
        
        permissionsCache.Set(user.Id ,flatPermissions);

        var refreshToken = tokenGenerator.GenerateRefreshToken();

        return new SuccessLoginResponse
        {
            Status = user.Status.ToString(),
            AuthProvider = user.AuthProvider.ToString(),
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expirationMinutes * 60,
            Branches = branchesResponse,
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