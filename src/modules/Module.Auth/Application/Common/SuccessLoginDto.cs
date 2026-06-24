using Common.Contracts.authentication.dtos;
using Module.Auth.Application.UseCases.Users.GetAllUsers;

namespace Module.Auth.Application.Common;

public class SuccessLoginResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AuthProvider { get; set; } = string.Empty;
    public UserDetailResponse User { get; set; } = default!;
    public List<PermissionsByModuleDto> Branches { get; set; } = new List<PermissionsByModuleDto>();

}

