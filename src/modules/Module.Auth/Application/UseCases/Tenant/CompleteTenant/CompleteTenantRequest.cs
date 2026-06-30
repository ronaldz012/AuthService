namespace Module.Auth.Application.UseCases.Tenant.CompleteTenant;

public record CompleteTenantRequest(
    string Token,
    string Password
);