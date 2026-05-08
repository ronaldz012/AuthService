namespace shared.Contracts.dtos;

public record CreateTenantDto(
    string DisplayName, 
    string Schema, 
    string? DatabaseName, 
    string AdminEmail, 
    string AdminPassword
);