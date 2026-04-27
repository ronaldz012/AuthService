using Auth.Contracts.Dtos.permissions;

namespace Auth.Contracts.Dtos.Users;

public class SuccessLoginDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AuthProvider { get; set; } = string.Empty;
    public UserDetailsDto User { get; set; } = default!;
    public List<PermissionsByModuleDto> Branches { get; set; } = new List<PermissionsByModuleDto>();

}

