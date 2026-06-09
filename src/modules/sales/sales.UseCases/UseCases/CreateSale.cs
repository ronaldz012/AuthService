using Auth.Contracts.Interfaces;
using Inventory.Contracts.interfaces;
using sales.Contracts.dtos;
using sales.Module.Data;
using sales.use.Entities;
using Common.Result;

namespace sales.UseCases.UseCases;

public class CreateSale(SalesDbContext context, ICurrentUser currentUser, IInventoryIntegrationService inventoryService)
{
    public async Task<Result<bool>> Execute(CreateSaleDto dto)
    {
        var branchId = currentUser.BranchIds[0];
        var variantIds = dto.Items.Select(i => i.ProductVariantId).Distinct().ToList();

        var stockResult = await inventoryService.GetVariantsWithStock(variantIds, branchId);
        if (!stockResult.IsSuccess) return stockResult.Error;

        var variants = stockResult.Value;
        if (variants.Count != variantIds.Count)
            return new Error("NOT_FOUND", "Uno o más productos no existen");

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {

            var sale = new Sale
            {
                Id = new Guid(),
                BranchId = branchId,
                SoldById = currentUser.UserId,
                PaymentMethod = dto.PaymentMethod,
                TransactionCode = dto.TransactionCode,
                Status = SaleStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };
            // 1. Descontar stock + crear movements (sin SaveChanges aún)
            var deductions = dto.Items
                .Select(i => new StockDeductionDto(i.ProductVariantId, i.Quantity))
                .ToList();

            var deductResult = await inventoryService.DeductStock(deductions, branchId, currentUser.UserId, sale.Id);
            if (!deductResult.IsSuccess) return deductResult.Error;

            // 2. Construir y guardar Sale
        

            foreach (var itemDto in dto.Items)
            {
                var variant = variants.First(v => v.Id == itemDto.ProductVariantId);
                var subtotal = (variant.Price - itemDto.DiscountAmount) * itemDto.Quantity;

                sale.SaleItems.Add(new SaleItem
                {
                    ProductVariantId = variant.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = variant.Price,
                    DiscountAmount = itemDto.DiscountAmount,
                    FinalPrice = subtotal
                });

                sale.TotalAmount += subtotal;
            }

            context.Sales.Add(sale);
            await context.SaveChangesAsync(); // persiste todo junto

            await transaction.CommitAsync();
            return true;
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return new Error("VALIDATION_ERROR", ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}