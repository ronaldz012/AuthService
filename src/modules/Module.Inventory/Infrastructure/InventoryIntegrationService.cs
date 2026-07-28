using Common.Contracts.inventory;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Infrastructure;

public class InventoryIntegrationService(IInvDbContext context) : IInventoryIntegrationService
{
    public async Task<Result<List<ProductVariantStockDto>>> GetVariantsWithStock(List<Guid> variantIds, Guid branchId)
    {
        var result = await context.ProductVariants
            .Where(pv => variantIds.Contains(pv.Id))
            .Select(pv => new ProductVariantStockDto(
                pv.Id,
                pv.Sku,
                // Inventory compone el nombre mostrable; Sales lo guarda tal cual como snapshot
                pv.Product.Brand.Name + " " + pv.Product.Category.Name + " " + pv.Product.Name
                    + " - " + pv.Color.Name + " / " + pv.Size,
                pv.Price,
                pv.BranchInventories
                    .Where(bi => bi.BranchId == branchId)
                    .Select(bi => bi.Stock)
                    .FirstOrDefault()
            ))
            .ToListAsync();

        return result;
    }

public async Task<Result<bool>> DeductStock(
    List<StockDeductionDto> deductions, Guid branchId, Guid userId, string userName, Guid referenceId)
{
    var variantIds = deductions.Select(d => d.ProductVariantId).ToList();

    var variants = await context.ProductVariants
        .Include(pv => pv.BranchInventories.Where(bi => bi.BranchId == branchId))
        .Where(pv => variantIds.Contains(pv.Id))
        .ToListAsync();

    foreach (var deduction in deductions)
    {
        var pv = variants.FirstOrDefault(v => v.Id == deduction.ProductVariantId);
        if (pv == null)
        {
            return new Error(ErrorCode.NotFound, $"Product variant {deduction.ProductVariantId} not found.");
        }
        if (!pv.HasSufficientStock(deduction.Quantity, branchId))
        {
            return new Error(ErrorCode.InvalidState, $"Insufficient stock for product {pv.Sku}.");
        }
        pv.SellStock(deduction.Quantity, branchId, userId, userName, referenceId);
    }

    return true; 
}

    public async Task<bool> BranchHasPendingTransfers(Guid branchId)
    {
        return await context.StockTransfers
            .AnyAsync(t =>
                (t.FromBranchId == branchId || t.ToBranchId == branchId) &&
                (t.Status == TransferStatus.Pending || t.Status == TransferStatus.Transit));
    }
}