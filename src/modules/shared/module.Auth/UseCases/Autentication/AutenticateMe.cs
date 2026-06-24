using Common.Contracts.authentication;
using Common.Contracts.branches;
using Common.permissions;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using module.Auth.dtos.Users;
using module.Auth.interfaces;

namespace module.Auth.Features.Autentication;

public class AutenticateMe(
    AuthDbContext context,
    ICurrentUser currentUser,
    IBranchService branchService,
    IUserPermissionsCacheService permissionsCache)  
{
    public async Task<Result<SuccessLoginDto>> Execute()
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
    
            // Agrupamos las features de este Branch por su Módulo
            Modules = b.Features
                .GroupBy(f => new {f.Name })
                .Select(gModule => new PermissiónByModuleDto
                {
                    Name = gModule.Key.ModuleName,
                    Description = string.Empty,
                    Route = gModule.Key.ModuleName,    
                    Icon = string.Empty,       
            
                    Features = gModule.Select(f => new FeaturePermissionByModuleDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Route = f.Route,
                        Icon = string.Empty, 
                        CanCreate = f.CanCreate,
                        CanRead = f.CanRead,
                        CanUpdate = f.CanUpdate,
                        CanDelete = f.CanDelete
                    }).ToList()
                }).ToList()
        }).ToList();

        return new SuccessLoginDto
        {
            Status       = user.Status.ToString(),
            AuthProvider = user.AuthProvider.ToString(),
            Branches     = branches,
            User         = new UserDetailsDto
            {
                Id    = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            }
        };
    }
}