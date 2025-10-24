using Auth.Data.Entities;
using Auth.Data.Persistence;
using Auth.Dtos.Users;
using Auth.Infrastructure.Authentication;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Result;
namespace Auth.UseCases.Users;

public class RegisterUser(AuthDbContext dbContext, IMapper mapper, IOptions<AuthenticationSettings> authSettings)
{
    private readonly IOptions<AuthenticationSettings> _authSettings = authSettings;
    public async Task<Result<bool>> Execute(RegisterUserDto dto)
    {
        var emailAndUserNameAvailable = await dbContext.Users
        .AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username);
        if (emailAndUserNameAvailable)
        {
            return new Error("DUPLICATE", "El correo electrónico o el nombre de usuario ya están en uso.");
        }
        var emailVerificationRequired = _authSettings.Value.EmailVerification.Required &&
            _authSettings.Value.EmailVerification.RequiredForProviders.Contains("Local");

        
        var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var userToCreate = mapper.Map<User>(dto); 
            byte[] passwordHash, passwordSalt;
            ValidatePassword.CreatePasswordHash(dto.Password, out passwordHash, out passwordSalt);
            userToCreate.PasswordHash = passwordHash;
            userToCreate.PasswordSalt = passwordSalt;

            if(emailVerificationRequired)
            {
                
            }


            await dbContext.Users.AddAsync(userToCreate);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch 
        {
            await transaction.RollbackAsync();
            throw;
        }
    }



    
}
