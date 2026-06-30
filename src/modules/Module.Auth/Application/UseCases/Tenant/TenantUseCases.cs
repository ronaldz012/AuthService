using Module.Auth.Application.UseCases.Tenant.Create;

namespace Module.Auth.Application.UseCases.Tenant;

public record TenantUseCases(CreateTenant CreateTenant, CompleteTenant.CompleteTenant  CompleteTenant );