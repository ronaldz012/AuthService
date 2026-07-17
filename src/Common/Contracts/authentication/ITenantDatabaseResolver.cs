using Common.Contracts.authentication.dtos;

namespace Common.Contracts.authentication;

public interface ITenantDatabaseResolver
{
    Task<TenantDatabaseInfoDto?> GetTenantDatabaseInfo(Guid tenantId);
    Task<TenantDatabaseInfoDto?> GetByDisplayName(string displayName);
}
