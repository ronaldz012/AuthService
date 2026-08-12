using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Receptions.GetById;

public class GetReception(IInvDbContext context, ICurrentUser currentUser)
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
                ProviderId = r.ProviderId,
                ProviderName = r.Provider.Name,
                ReceivedAt = r.ReceivedAt,
                Notes = r.Notes,
                Status = r.Status,
                TotalCost = r.Items.Sum(i => i.UnitCost * i.QuantityReceived),
                Items = r.Items.Select(i => new StockReceptionItemDetailDto
                {
                    Id = i.Id,
                    Sku = i.ProductVariant.Sku,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductVariant.Product.Name,
                    VariantDescription = i.ProductVariant.Description,
                    Size = i.ProductVariant.Size.Name,
                    Color = i.ProductVariant.Color.Name,
                    QuantityReceived = i.QuantityReceived,
                    UnitCost = i.UnitCost,
                    Subtotal = i.UnitCost * i.QuantityReceived
                }).OrderBy(i => i.ProductName).ToList()
            })
            .FirstOrDefaultAsync();
        if(reception == null)
            return GetReceptionErrors.ReceptionNotFound;
        
        return reception;
    }
}