using Auth.Contracts.Dtos.Users;
using Auth.Contracts.Interfaces;
using Auth.Data.Entities;
using Auth.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace Auth.UseCases.Users;

public class UserIntegrationService(AuthDbContext context) : IUserIntegrationService
{
    public async Task<Result<List<UserDetailsDto>>> GetUsersByIds(List<int> userIds)
    {
        var usersFound = await context.Users.IgnoreQueryFilters().Where(x => userIds.Contains(x.Id)).Select(u => new UserDetailsDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            DeletedAt = u.DeletedAt,
        }).ToListAsync();
        
        var missingUsersIds =userIds.Except(usersFound.Select(u => u.Id));
        if (missingUsersIds.Any()) return new Error("NOT_FOUND", $"Users with Ids not found{missingUsersIds.ToString()}");
        return usersFound;
    }
}