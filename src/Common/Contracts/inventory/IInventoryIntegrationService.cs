using Common.Utilities;

namespace Common.Contracts.inventory;

public interface IInventoryIntegrationService
{
    Task<Result<List<ProductVariantStockDto>>> GetVariantsWithStock(
        List<Guid> variantIds, Guid branchId);

    Task<Result<bool>> DeductStock(
        List<StockDeductionDto> deductions, Guid branchId, Guid userId, string userName, Guid referenceId);

    Task<bool> BranchHasPendingTransfers(Guid branchId);
}

public record ProductVariantStockDto(
    Guid Id,
    string Sku,
    string DisplayName,
    decimal Price,
    int Stock,
    bool IsActive,
    decimal AverageCost);

public record StockDeductionDto(
    Guid ProductVariantId,
    int Quantity);