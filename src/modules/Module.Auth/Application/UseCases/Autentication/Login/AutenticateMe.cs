using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.Contracts.branches;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Common;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Users.GetAllUsers;

namespace Module.Auth.Application.UseCases.Autentication.Login;

public class AutenticateMe(
    IAuthDbContext context,
    ICurrentUser currentUser,
    IBranchService branchService,
    IUserPermissionsCacheService permissionsCache)  
{
    public async Task<Result<SuccessLoginResponse>> Execute()
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId);

        if (user == null)
            return new Error("NOT_FOUND", "User Not Found");

        var isAdmin = user.IsAdmin;
        List<PermissionsDto> userBranchesCache = await permissionsCache.GetAsync(user.Id, isAdmin);


        List<PermissionsByModuleDto> branches = userBranchesCache.Select(b => new PermissionsByModuleDto
        {
            BranchId = b.BranchId,
            BranchName = b.BranchName,
            Roles = b.Roles,
    
            Modules = b.Features
                .GroupBy(f => new {f.ModuleName })
                .Select(gModule => new PermissiónByModuleDto
                {
                    Name = gModule.Key.ModuleName,
                    Description = string.Empty,
                    Route = gModule.Key.ModuleName,    
                    Icon = string.Empty,       
            
                    Features = gModule.Select(f => new FeaturePermissionByModuleDto
                    {
                        Id = f.Id,
                        Permission = f.Permissions,
                    }).ToList()
                }).ToList()
        }).ToList();

        return new SuccessLoginResponse
        {
            Status       = user.Status.ToString(),
            AuthProvider = user.AuthProvider.ToString(),
            Branches     = branches,
            User         = new UserDetailResponse
            {
                Id    = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            }
        };
    }
}