using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Infrastructure.Authentication;

public class TenantDatabaseResolverService(
    IAuthDbContext context,
    IMemoryCache cache) : ITenantDatabaseResolver
{
    private static readonly MemoryCacheEntryOptions CacheOpts =
        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    private static string Key(Guid tenantId) => $"tenant_database:{tenantId}";

    public async Task<TenantDatabaseInfoDto?> GetTenantDatabaseInfo(Guid tenantId)
    {
        if (cache.TryGetValue(Key(tenantId), out TenantDatabaseInfoDto? cached) && cached is not null)
            return cached;

        var info = await context.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => new TenantDatabaseInfoDto
            {
                Schema = t.TenantDataBase.Schema,
                DatabaseName = t.TenantDataBase.Name
            })
            .FirstOrDefaultAsync();

        if (info is not null)
            cache.Set(Key(tenantId), info, CacheOpts);

        return info;
    }
}
