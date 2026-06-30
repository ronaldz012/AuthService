using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.TenantDatabases.Get;

public class GetTenantDatabases(IAuthDbContext context)
{
    public async Task<Result<List<TenantDatabaseResponse>>> Execute()
    {
        var dbs = context.TenantDatabases.Select(x => new TenantDatabaseResponse()
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Schema = x.Schema,
        }).ToListAsync();
        return await dbs;
    }
}