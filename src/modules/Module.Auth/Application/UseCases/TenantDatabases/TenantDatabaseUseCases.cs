using Module.Auth.Application.UseCases.TenantDatabases.Get;
using Module.Auth.Application.UseCases.TenantDatabases.GetById;

namespace Module.Auth.Application.UseCases.TenantDatabases;

public record TenantDatabaseUseCases
(GetTenantDatabases GetTenantDatabases,GetTenantDatabaseDetails GetTenantDatabaseDetails);