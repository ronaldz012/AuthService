using Auth.Contracts.Interfaces;
using Inventory.Contracts.Dtos.Receptions;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Receptions;

public class GetReception(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<StockReceptionDetailDto>> Execute(Guid id)
    {
        var currentBranch = currentUser.BranchIds[0];
       
        var reception =  await context.StockReceptions
            .Where(r => r.Id == id &&  r.BranchId == currentBranch)
            .Select(r => new StockReceptionDetailDto
            {
                Id = r.Id,
                BranchId = r.BranchId,
                ReceivedAt = r.ReceivedAt,
                Notes = r.Notes,
                Status = r.Status,
                TotalCost = r.Items.Sum(i => i.UnitCost * i.QuantityReceived),
                Items = r.Items.Select(i => new StockReceptionItemDetailDto
                {
                    Id = i.Id,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductVariant.Product.Name,
                    VariantDescription = i.ProductVariant.Description,
                    Size = i.ProductVariant.Size,
                    Color = i.ProductVariant.Color,
                    QuantityReceived = i.QuantityReceived,
                    UnitCost = i.UnitCost,
                    Subtotal = i.UnitCost * i.QuantityReceived
                }).ToList()
            })
            .FirstOrDefaultAsync();
        if(reception == null)
            return new Error("NOT_FOUND", "Reception not found");
        
        var variantIds = reception.Items.Select(i => i.ProductVariantId).ToList();

        var itemsActualStock = await context.ProductVariants
            .Where(pv => variantIds.Contains(pv.Id))
            .Select(pv => new
            {
                VariantId = pv.Id,
                StockInBranch = pv.BranchInventories
                    .Where(bi => bi.BranchId == currentBranch)
                    .Select(bi => bi.Stock)
                    .FirstOrDefault() // Si no existe registro, devuelve 0 automáticamente
            })
            .ToDictionaryAsync(x => x.VariantId, x => x.StockInBranch);

        bool hasEnoughStock = true;

        foreach (var item in reception.Items)
        {
            itemsActualStock.TryGetValue(item.ProductVariantId, out var currentStock);
            if (currentStock < item.QuantityReceived)
            {
                reception.ReasonCannotRollback = "NOT_ENOUGH_STOCK";
                hasEnoughStock = false;
                break;
            }
        }

        if (reception.ReceivedAt < DateTime.Now.AddDays(-1))
        {
            reception.CanRollBack = false;
            reception.ReasonCannotRollback = "OUTDATED";
        }
        else if (!hasEnoughStock)
        {
            reception.CanRollBack = false;
            reception.ReasonCannotRollback = "NOT_ENOUGH_STOCK";
        }
        else
        {
            reception.CanRollBack = true;
        }
        return reception;
        

    }
    
}