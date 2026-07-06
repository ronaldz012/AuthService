namespace Module.Auth.Application.UseCases.Tenant.Create;

public record CreateTenantResponse(string Code, string SetupUrl, string DisplayName);
