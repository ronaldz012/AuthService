using Auth.Contracts.Interfaces;
using Inventory.Contracts.Dtos.Products;
using Inventory.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace Inventory.UseCases.Products;

public class ProductDetails(InvDbContext  context, ICurrentUser currentUser)
{
    public async Task<Result<ProductDetailDto>> Execute(int productId)
    {
        var branchNames = await currentUser.GetBranchNamesAsync();
         
        var result =   await context.Products.Select(p => new ProductDetailDto
        {
            Id = p.Id,
            Name = p.Name,
            InternalCode = p.InternalCode,
            Description = p.Description,
            BasePrice = p.BasePrice,
            Gender = p.Gender.ToString(),
            CategoryName = p.Category.Name,
            BrandName = p.Brand.Name,
            Variants = p.ProductVariants.Select(pv => new ProductVariantDetailDto
            {
                Id = pv.Id,
                Sku = pv.Sku,
                Description = pv.Description,
                Size = pv.Size,
                Color = pv.Color,
                Price = pv.Price,
                Stock = pv.BranchInventories.Where(x => currentUser.BranchIds.Contains(x.BranchId)).Select(x => new StockDto
                {
                    BranchId = x.BranchId,
                    BranchName = branchNames[x.BranchId] ?? string.Empty,
                    Stock = x.Stock
                }).ToList()
            }).ToList()
        }).FirstOrDefaultAsync(x  => x.Id == productId);
        
        if(result is null) return new Error("NOT_FOUND", "Productos no encontrado");

        foreach (var variant in result.Variants)
        {
            variant.StockOfVariantinBranches = variant.Stock.Sum(s => s.Stock);
        }

        result.TotalStockInBranches = result.Variants.Sum(v => v.StockOfVariantinBranches);
        return result;
    }
}