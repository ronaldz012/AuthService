using System;
using Auth.Data.Entities;
using Auth.Data.Persistence;
using Auth.Dtos.Users;
using Auth.Infrastructure.Authentication;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace Auth.UseCases.Users;

public class RegisterUser(AuthDbContext dbContext, ITokenGenerator tokenGenerator, IMapper mapper)
{
    public async Task<Result<bool>> Execute(RegisterUserDto dto)
    {
        var emailAndUserNameAvailable = await dbContext.Users
        .AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username);
        if (emailAndUserNameAvailable)
        {
            return new Error("DUPLICATE", "El correo electrónico o el nombre de usuario ya están en uso.");
        }
        var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var userToCreate = mapper.Map<User>(dto);
            byte[] passwordHash, passwordSalt;
            ValidatePassword.CreatePasswordHash(dto.Password, out passwordHash, out passwordSalt);
            userToCreate.PasswordHash = passwordHash;
            userToCreate.PasswordSalt = passwordSalt;
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



    private  static class ValidatePassword
    {
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
                return true;
            }
        }
    }
}
