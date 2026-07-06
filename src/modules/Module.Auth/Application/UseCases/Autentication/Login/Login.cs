using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.Common;
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
        var user = await dbContext.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return LoginErrors.UserNotFound;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return LoginErrors.InvalidPassword;


        tenantContext.TenantId = user.TenantId;
        
        List<PermissionsDto> flatPermissions = await permissionsCache.GetAsync(user.Id, user.TenantId, user.IsAdmin);
        
        var branches = new List<PermissionsByModuleDto>();

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
            user.IsAdmin);
        
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
                IsAdmin = user.IsAdmin,
                UserType = user.Type,
                LastName = user.LastName,
                Email = user.Email
            }
        };
    }
}