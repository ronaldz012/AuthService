using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Products.GetById;

public class ProductDetails(IInvDbContext context)
{
    public async Task<Result<ProductDetailDto>> Execute(ActorContext ctx, Guid productId)
    {
        var userBranches = ctx.BranchIds;

        var result = await context.Products.Select(p => new ProductDetailDto
        {
            Id = p.Id,
            Name = p.Name,
            InternalCode = p.InternalCode,
            Description = p.Description,
            BasePrice = p.BasePrice,
            Gender = p.Gender,
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            BrandId = p.BrandId,
            BrandName = p.Brand.Name,
            Variants = p.ProductVariants.Select(pv => new ProductVarianListDto
            {
                Id = pv.Id,
                Sku = pv.Sku,
                Description = pv.Description,
                Size = pv.Size.Name,
                SizeId = pv.SizeId,
                Color = pv.Color.Name,
                ColorId = pv.ColorId,
                Price = pv.Price,
                BranchStocks = pv.BranchInventories
                    .Where(bi => userBranches.Contains(bi.BranchId))
                    .Select(bi => new BranchStockDto
                    {
                        BranchId = bi.BranchId,
                        Stock = bi.Stock,
                    }).ToList()
            }).ToList()
        }).FirstOrDefaultAsync(x => x.Id == productId);

        if (result is null) return ProductDetailsErrors.ProductNotFound;

        result.TotalAvailable = result.Variants.Sum(v => v.TotalAvailable);
        return result;
    }
}