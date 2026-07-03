using Common.Utilities;

namespace Module.Auth.Application.UseCases.TenantDatabases.GetById;

public static class GetTenantDatabaseDetailsErrors
{
    public static readonly Error DatabaseNotFound = new(ErrorCode.NotFound, "Database not found");
}
