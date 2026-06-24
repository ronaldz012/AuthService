using Common.Utilities;
using module.Auth.dtos.Users;
using module.Auth.interfaces;

namespace module.Auth.Users;

public class UserIntegrationService(AuthDbContext context) : IUserIntegrationService
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
        return usersFound;
    }

    public async Task<Result<Guid>> CreateTenantAdminAsync(string email, string password)
    {
            if (await context.Users.AnyAsync(u => u.Email == email))
                return new Error("CONFLICT", "El email ya está registrado.");

            ValidatePassword.CreatePasswordHash(password, out var hash, out var salt);

            var user = new User
            {
                Email        = email,
                Username     = email, // o separar si tienes username
                PasswordHash = hash,
                PasswordSalt = salt,
                IsAdmin      = true,       // ← flag de admin
                Status       = UserStatus.Active,
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(); // usa el schema del tenantContext ya seteado

            return user.Id;
        
    }
    
}