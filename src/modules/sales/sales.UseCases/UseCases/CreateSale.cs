using Auth.Contracts.Interfaces;
using Inventory.Contracts.Dtos.Sales;
using Inventory.Data.Entities.Inventory;
using Inventory.Data.Entities.sales;
using Inventory.Data.Persistence;
using Inventory.Data.Persistence.Migrations;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

public class CreateSale(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(CreateSaleDto createSaleDto)
    {
        var currentBranch = currentUser.BranchIds[0];
        var productIds = createSaleDto.Items.Select(i => i.ProductVariantId).ToList();


        var productVariants = await context.ProductVariants
            .Include(pv => pv.BranchInventories.FirstOrDefault(bi => bi.BranchId == currentBranch))
            .Where(pv => productIds.Contains(pv.Id))
            .ToListAsync();

        // 2. Validaciones iniciales
        if (productVariants.Count != productIds.Distinct().Count())
            return new Error("NOT_FOUND", "Uno o más productos no existen");

        var sale = new Sale
        {
            BranchId = currentBranch,
            SoldById = currentUser.UserId,
            PaymentMethod = createSaleDto.PaymentMethod,
            TransactionCode = createSaleDto.TransactionCode,
            Status = SaleStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };
        var movements = new List<StockMovement>();
        try 
        {
            foreach (var itemDto in createSaleDto.Items)
            {
                var pv = productVariants.First(p => p.Id == itemDto.ProductVariantId);
                pv.RemoveQuantity(itemDto.Quantity, currentBranch);

                // Cálculo de precios
                var subTotal = (pv.Price - itemDto.DiscountAmount) * itemDto.Quantity;
                
                sale.SaleItems.Add(new SaleItem
                {
                    ProductVariantId = pv.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = pv.Price,
                    DiscountAmount = itemDto.DiscountAmount,
                    FinalPrice = subTotal
                });
                movements.Add(StockMovement.CreateSale(currentBranch,pv.Id,currentUser.UserId,itemDto.Quantity));
                

                sale.TotalAmount += subTotal;
            }
        }
        catch (InvalidOperationException ex)
        {
            return new Error("VALIDATION_ERROR", ex.Message);
        }

        // 3. Persistencia
        context.StockMovements.AddRange(movements);
        context.Sales.Add(sale);
        return await context.SaveChangesAsync() > 0;
    }
}