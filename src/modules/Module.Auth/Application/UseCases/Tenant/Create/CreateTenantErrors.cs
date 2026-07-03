using Common.Utilities;

namespace Module.Auth.Application.UseCases.Tenant.Create;

public static class CreateTenantErrors
{
    public static readonly Error DatabaseNotFound = new(ErrorCode.NotFound, "Database not found");
    public static readonly Error TenantAlreadyExists = new(ErrorCode.ValidationError, "Tenant already exists");
    public static readonly Error PlanNotFound = new(ErrorCode.NotFound, "The specified subscription plan does not exist");
}
