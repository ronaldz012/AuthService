using Inventory.Contracts.interfaces;
using Inventory.Data.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases;

public class InventoryIntegrationService(InvDbContext context) : IInventoryIntegrationService
{
    public async Task<Result<List<ProductVariantStockDto>>> GetVariantsWithStock(
        List<Guid> variantIds, Guid branchId)
    {
        var variants = await context.ProductVariants
            .Include(pv => pv.BranchInventories.Where(bi => bi.BranchId == branchId))
            .Where(pv => variantIds.Contains(pv.Id))
            .ToListAsync();

        var result = variants.Select(pv => new ProductVariantStockDto(
            pv.Id,
            pv.Sku,
            pv.Price,
            pv.BranchInventories.FirstOrDefault()?.Stock ?? 0
        )).ToList();

        return result;
    }

    public async Task<Result<bool>> DeductStock(
        List<StockDeductionDto> deductions, Guid branchId, Guid userId)
    {
        var variantIds = deductions.Select(d => d.ProductVariantId).ToList();

        var variants = await context.ProductVariants
            .Include(pv => pv.BranchInventories.Where(bi => bi.BranchId == branchId))
            .Where(pv => variantIds.Contains(pv.Id))
            .ToListAsync();

        var movements = new List<StockMovement>();

        foreach (var deduction in deductions)
        {
            var pv = variants.First(v => v.Id == deduction.ProductVariantId);
            pv.RemoveQuantity(deduction.Quantity, branchId); // lanza si stock insuficiente
            movements.Add(StockMovement.CreateSale(branchId, pv.Id, userId, deduction.Quantity));
        }

        context.StockMovements.AddRange(movements);
        // No llama SaveChanges, eso lo maneja CreateSale dentro de la transacción
        return true;
    }
}