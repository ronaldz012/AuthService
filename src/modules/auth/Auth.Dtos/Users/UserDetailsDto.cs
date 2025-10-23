using System;

namespace Auth.Dtos.Users;

public class UserDetailsDto
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FatherLastName { get; set; } = string.Empty;
    public string MotherLastName { get; set; } = string.Empty;
    
}
