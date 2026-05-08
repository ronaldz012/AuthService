using Inventory.Contracts.Dtos.Receptions;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Receptions;

public class ReceptionLabels(InvDbContext context)
{
    public async Task<Result<ReceptionLabelsDto>> Execute(Guid receptionId)
    {
        var result=  await context.StockReceptions.Where(r => r.Id == receptionId).Select(r => new ReceptionLabelsDto
        {
            ReceptionId = r.Id,
            ReceptionDate = r.ReceivedAt,
            Items = r.Items.Select(ri =>new ReceptionLabelItemDto
            {
                VariantId = ri.ProductVariantId,
                Sku = ri.ProductVariant.Sku,
                Size = ri.ProductVariant.Size,
                Color = ri.ProductVariant.Color,
                Gender = ri.ProductVariant.Product.Gender,
                Price = ri.ProductVariant.Price,
                ProductName = ri.ProductVariant.Product.Name+"/"+ri.ProductVariant.Description +"/"+ri.ProductVariant.Description,
                BrandName = ri.ProductVariant.Product.Brand.Name,
                CategoryName = ri.ProductVariant.Product.Category.Name,
                Quantity = ri.QuantityReceived
            }).ToList()
        }).FirstOrDefaultAsync();

        if (result is null) return new Error("NOT_FOUND", "reception not found");
        
        return result;
    }

}
