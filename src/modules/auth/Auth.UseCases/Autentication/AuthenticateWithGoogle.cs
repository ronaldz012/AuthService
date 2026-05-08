using Auth.Contracts.Dtos.Users;
using Auth.Data;
using Auth.Data.Entities;
using Auth.Infrastructure.Authentication;
using Auth.UseCases.Autentication.functions;
using Branches.Contracts;
using Branches.module.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Common.Data;
using Common.Result;
using shared.Contracts.interfaces;

namespace Auth.UseCases.Autentication;
public class AuthenticateWithGoogle(
    AuthDbContext dbContext,
    RegisterUser registerUser,
    IMapper mapper,
    IGoogleTokenValidator googleTokenValidator,
    ITokenGenerator tokenGenerator,
    IBranchService branchService,
    IFeatureService featureService,
    ITenantContext tenantContext)
{
    public async Task<Result<SuccessLoginDto>> Execute(string idToken)
    {
        return new Error("NOT_IMPLEMENTED", "in maintenance");
    }
    //     var googleUserResult = await googleTokenValidator.ValidateTokenAsync(idToken);
    //     if (!googleUserResult.IsSuccess)
    //         return googleUserResult.Error!;
    //
    //     var googleUser = googleUserResult.Value;
    //
    //     var existingUser = await dbContext.Users
    //         .AsSplitQuery()
    //         .Include(u => u.UserBranchRoles)
    //             .ThenInclude(ur => ur.Role)
    //                 .ThenInclude(r => r.RoleFeaturePermissions)
    //         .FirstOrDefaultAsync(u => u.Email == googleUser!.Email);
    //
    //     if (existingUser == null)
    //     {
    //         var createResult = await CreateGoogleUser(googleUser!);
    //         if (!createResult.IsSuccess)
    //             return createResult.Error!;
    //
    //         existingUser = createResult.Value;
    //     }
    //
    //     var featureIds = existingUser!.UserBranchRoles
    //         .SelectMany(ubr => ubr.Role.RoleFeaturePermissions)
    //         .Select(rmp => rmp.FeatureId)
    //         .Distinct();
    //
    //     var features = await featureService.GetFeaturesByIdsAsync(featureIds);
    //     var featureMap = features.ToDictionary(f => f.Id);
    //
    //     var branchesResult = await UserMappingUtils.BuildBranchAccessByModule(existingUser, branchService, featureMap);
    //     if (!branchesResult.IsSuccess)
    //         return branchesResult.Error!;
    //
    //     var accessToken  = tokenGenerator.GenerateAccessToken(existingUser.Id, tenantContext.Schema?? "");
    //     var refreshToken = tokenGenerator.GenerateRefreshToken();
    //
    //     return new SuccessLoginDto
    //     {
    //         AccessToken  = accessToken,
    //         RefreshToken = refreshToken,
    //         AuthProvider = existingUser.AuthProvider.ToString(),
    //         Status       = existingUser.Status.ToString(),
    //         User         = mapper.Map<UserDetailsDto>(existingUser),
    //         Branches     = branchesResult.Value,
    //     };
    // }
    //
    // private async Task<Result<User>> CreateGoogleUser(GoogleUserInfo googleUser)
    // {
    //     var roleResult = await registerUser.GetDefaultUserRole();
    //     if (!roleResult.IsSuccess)
    //         return roleResult.Error!;
    //
    //     var user = new User
    //     {
    //         Email          = googleUser.Email,
    //         Username       = googleUser.Email.Split('@')[0],
    //         FirstName      = googleUser.GivenName,
    //         LastName       = googleUser.FamilyName,
    //         Status         = UserStatus.PendingRoleSelecting,
    //         AuthProvider   = AuthProvider.Google,
    //         ExternalAuthId = googleUser.GoogleId,
    //         UserBranchRoles = new List<UserBranchRole>
    //         {
    //             new() { RoleId =Guid.Empty }
    //         }
    //     };
    //
    //     await dbContext.Users.AddAsync(user);
    //     await dbContext.SaveChangesAsync();
    //
    //     return user;
    // }
}