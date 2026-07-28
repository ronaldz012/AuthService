using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Npgsql;

namespace Module.Inventory.Infrastructure.Services;

public class ProductCodeService(IInvDbContext context, ITenantConnectionContext tenantContext) : IProductCodeService
{
    public async Task<string> ReserveBrandCounter(Guid brandId, string prefix)
    {
        var sql = $"""
                   UPDATE "{tenantContext.Schema}"."Brands"
                   SET "ProductCounter" = "ProductCounter" + 1
                   WHERE "Id" = @id
                   RETURNING "ProductCounter"
                   """;
        var result = await context.Database
            .SqlQueryRaw<int>(sql, new NpgsqlParameter("id", brandId))
            .ToListAsync();
        return $"{prefix}-{result[0]}";
    }

    public async Task<string> ReserveVariantCounter(Guid productId, string productCode)
    {
        var sql = $"""
                   UPDATE "{tenantContext.Schema}"."Products"
                   SET "ProductVariantCounter" = "ProductVariantCounter" + 1
                   WHERE "Id" = @id
                   RETURNING "ProductVariantCounter"
                   """;
        var result = await context.Database
            .SqlQueryRaw<int>(sql, new NpgsqlParameter("id", productId))
            .ToListAsync();
        return $"{productCode}-{result[0].ToString().PadLeft(3, '0')}";
    }
}
