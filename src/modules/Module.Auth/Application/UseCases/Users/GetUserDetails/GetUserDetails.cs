using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Users.GetUserDetails;

public class GetUserDetails(IAuthDbContext context)
{
    public async Task<Result<GetUserDetailsResponse>> Execute(Guid id)
    {
        var result = await context.Users
            .Where(u => u.Id == id)
            .Select(u => new GetUserDetailsResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Ci = u.Ci,
                Nationality = u.Nationality,
                BirthDate = u.BirthDate,
                UserType = u.Type,
                IsAdmin = u.IsAdmin,
                Status = u.Status,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                BranchRoles = u.UserBranchRoles.Select(ubr => new UserBranchRoleDetailDto
                {
                    BranchId = ubr.BranchId,
                    BranchName = ubr.Branch.Name,
                    RoleId = ubr.RoleId,
                    RoleName = ubr.Role.Name,
                }).ToList(),
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return GetUserDetailsErrors.UserNotFound;

        return result;
    }
}
