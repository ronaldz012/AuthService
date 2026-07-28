using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.ProductVariants.Create;

public class CreateProductVariantUc(IInvDbContext context, ICurrentUser currentUser, IProductCodeService codeService)
{
    public async Task<Result<List<ProductVariantCreatedDto>>> Execute(Guid productId, List<CreateProductVariantDto> dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        if (dto.Count == 0)
            return CreateProductVariantErrors.EmptyVariantList;

        var hasDuplicatesInDto = dto
            .GroupBy(x => new { x.ColorId, Size = x.Size.Trim().ToLower() })
            .Any(g => g.Count() > 1);

        if (hasDuplicatesInDto)
            return CreateProductVariantErrors.DuplicateVariantsInRequest;

        var product = await context.Products
            .Select(x => new { x.Id, x.InternalCode })
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
            return CreateProductVariantErrors.ProductNotFound;

        var colorIdsInDto = dto.Select(x => x.ColorId).Distinct().ToList();

        var colorsDictionary = await context.Colors
            .Where(c => colorIdsInDto.Contains(c.Id))
            .Select(x => new { x.Id, x.Name })
            .ToDictionaryAsync(x => x.Id, x => x);

        if (colorsDictionary.Count != colorIdsInDto.Count)
        {
            var missingIds = colorIdsInDto.Where(id => !colorsDictionary.ContainsKey(id)).ToList();
            return CreateProductVariantErrors.ColorIdsNotFound;
        }

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
            return CreateProductVariantErrors.VariantAlreadyExists;

        await using var tx = await context.Database.BeginTransactionAsync();
        try
        {
            var variants = new List<ProductVariant>();
            foreach (var x in dto)
            {
                var sku = await codeService.ReserveVariantCounter(productId, product.InternalCode);
                variants.Add(new ProductVariant
                {
                    ProductId = productId,
                    ColorId = x.ColorId,
                    Size = x.Size.Trim(),
                    Description = x.Description,
                    Price = x.Price,
                    Sku = sku,
                    CreatedBy = currentUser.UserId,
                    CreatedByName = currentUser.FullName
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
}