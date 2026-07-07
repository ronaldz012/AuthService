using Common.Utilities;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.GetAllUsers;

public class GetUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public UserType UserType { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public bool IsActive { get; set; }
}


public class GetUsersResponse : PagedResultDto<GetUser>
{
    public int ActiveUsers { get; set; }
}

