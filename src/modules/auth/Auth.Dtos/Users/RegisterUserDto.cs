using System;

namespace Auth.Dtos.Users;

public class RegisterUserDto
{
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FatherLastName { get; set; } = string.Empty;
        public string MotherLastName { get; set; } = string.Empty;
        public IEnumerable<int> RoleIds { get; set; } = Enumerable.Empty<int>(); 
}
