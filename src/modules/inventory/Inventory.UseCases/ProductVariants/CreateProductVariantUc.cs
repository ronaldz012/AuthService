using Common.Data;
using Common.Result;
using Inventory.Contracts.Dtos.ProductVariants;
using Inventory.Data;
using Inventory.Data.Entities.Products;
using Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Inventory.UseCases.ProductVariants;

public class CreateProductVariantUc(InvDbContext context, ITenantContext tenantContext)
{
    public async Task<Result<List<ProductVariantCreatedDto>>> Execute(Guid productId, List<CreateProductVariantDto> dto)
    {
        // ── Validar lista no vacía ────────────────────────────────────────
        if (dto.Count == 0)
            return new Error("BAD_REQUEST", "The variant list cannot be empty.");

        // ── Validar duplicados dentro del DTO ─────────────────────────────
        var hasDuplicatesInDto = dto
            .GroupBy(x => new { x.ColorId, Size = x.Size.Trim().ToLower() })
            .Any(g => g.Count() > 1);

        if (hasDuplicatesInDto)
            return new Error("BAD_REQUEST", "There are duplicate variants (same size and color) in your request.");

        // ── Validar producto ──────────────────────────────────────────────
        var product = await context.Products
            .Select(x => new { x.Id, x.InternalCode })
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
            return new Error("NOT_FOUND", "Product not found.");

        // ── Validar y mapear colores ──────────────────────────────────────
        var colorIdsInDto = dto.Select(x => x.ColorId).Distinct().ToList();

        var colorsDictionary = await context.Colors
            .Where(c => colorIdsInDto.Contains(c.Id))
            .Select(x => new { x.Id, x.Name })
            .ToDictionaryAsync(x => x.Id, x => x);

        if (colorsDictionary.Count != colorIdsInDto.Count)
        {
            var missingIds = colorIdsInDto.Where(id => !colorsDictionary.ContainsKey(id)).ToList();
            return new Error("NOT_FOUND", $"The following Color IDs do not exist: {string.Join(", ", missingIds)}");
        }

        // ── Validar duplicados contra base de datos ───────────────────────
        var dtoCombinations = dto
            .Select(d => new { d.ColorId, Size = d.Size.Trim().ToLower() })
            .ToList();

        var colorIdsToCheck = dtoCombinations.Select(d => d.ColorId).Distinct().ToList();
        var sizesToCheck = dtoCombinations.Select(d => d.Size).Distinct().ToList();

        var existingVariants = await context.ProductVariants
            .Where(pv => pv.ProductId == productId
                      && colorIdsToCheck.Contains(pv.ColorId)
                      && sizesToCheck.Contains(pv.Size.ToLower()))
            .Select(pv => new { pv.ColorId, Size = pv.Size.ToLower() })
            .ToListAsync();

        var alreadyExists = existingVariants
            .Any(ev => dtoCombinations.Any(d => d.ColorId == ev.ColorId && d.Size == ev.Size));

        if (alreadyExists)
            return new Error("DUPLICATED", "One or more variants with the same size and color already exist for this product.");

        // ── Transacción: reservar SKUs y guardar ──────────────────────────
        await using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            var variants = new List<ProductVariant>();
            foreach (var x in dto)
            {
                var sku = await ReserveVariantCounter(productId, product.InternalCode);
                variants.Add(new ProductVariant
                {
                    ProductId = productId,
                    ColorId = x.ColorId,
                    Size = x.Size.Trim(),
                    Description = x.Description,
                    Price = x.Price,
                    Sku = sku
                });
            }

            context.ProductVariants.AddRange(variants);
            await context.SaveChangesAsync();
            await tx.CommitAsync();

            return variants.Select(v => new ProductVariantCreatedDto
            {
                ProductVariantId = v.Id,
                Sku = v.Sku,
                Size = v.Size,
                ColorName = colorsDictionary[v.ColorId].Name
            }).ToList();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Helper ────────────────────────────────────────────────────────────

    private async Task<string> ReserveVariantCounter(Guid productId, string productCode)
    {
        string schema = tenantContext.Schema;
        var sql = $"""
            UPDATE "{schema}"."Products"
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