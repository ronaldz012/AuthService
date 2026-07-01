namespace Common.Contracts.authentication.dtos;

public record CreateTenantDto(
    string DisplayName, 
    string Schema, 
    string? DatabaseName, 
    string AdminEmail, 
    string AdminPassword,
    string BranchName,
    string BranchPlace,
    string BranchPhoneNumber,
    string BranchCode
);

public class CreateTenantAdminDto
{
    
    public string Email  { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
   
