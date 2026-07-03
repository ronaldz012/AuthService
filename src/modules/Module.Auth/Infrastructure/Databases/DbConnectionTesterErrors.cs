using Common.Utilities;

namespace Module.Auth.Infrastructure.Databases;

public static class DbConnectionTesterErrors
{
    public static readonly Error ConnectionStringNotFound = new(ErrorCode.NotFound, "Connection string 'TenantConnection' not found");
    public static readonly Error SchemaNotFound = new(ErrorCode.NotFound, "Schema not found in database");
    public static readonly Error ConnectionFailed = new(ErrorCode.DatabaseError, "Database connection failed");
}
