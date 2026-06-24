using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users;

public class UserIntegrationService(IAuthDbContext context, ITokenGenerator tokenGenerator) : IUserIntegrationService
{
    public async Task<Result<List<UserDetailsDto>>> GetUsersByIds(List<Guid> userIds)
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
        return usersFound.Select(x => new UserDetailsDto()
        {
            Id = x.Id,
            Email = x.Email,
            DeletedAt = x.DeletedAt,
            FirstName = x.FirstName,
            LastName = x.LastName,
        }).ToList();
    }

    public async Task<Result<Guid>> CreateTenantAdminAsync(string email, string password)
    {
            if (await context.Users.AnyAsync(u => u.Email == email))
                return new Error("CONFLICT", "El email ya está registrado.");


            var user = new User
            {
                Email        = email,
                Username     = email, // o separar si tienes username
                PasswordHash = "",
                IsAdmin      = true,       // ← flag de admin
                Status       = UserStatus.Active,
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(); // usa el schema del tenantContext ya seteado

            return user.Id;
        
    }
    
}