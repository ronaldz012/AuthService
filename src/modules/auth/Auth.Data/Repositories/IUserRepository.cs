using Auth.Data.Entities;
using Auth.Dtos.Users;

namespace Auth.Data.Repositories;

public interface IUserRepository
{
    Task<UserDetailsDto?> GetUserById(int id);
    Task<CreateUserDto> AddUser(CreateUserDto user);
}
public class UserRepository : IUserRepository
{
    public Task<CreateUserDto> AddUser(CreateUserDto user)
    {
        throw new NotImplementedException();
    }
    public Task<UserDetailsDto?> GetUserById(int id)
    {
        throw new NotImplementedException();
    }

    
}