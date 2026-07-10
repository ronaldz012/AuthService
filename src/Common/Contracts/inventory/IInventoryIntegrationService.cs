using Common.Utilities;

namespace Common.Contracts.inventory;

public interface IInventoryIntegrationService
{
    Task<Result<List<ProductVariantStockDto>>> GetVariantsWithStock(
        List<Guid> variantIds, Guid branchId);

    Task<Result<bool>> DeductStock(
        List<StockDeductionDto> deductions, Guid branchId, Guid userId, Guid referenceId);

    Task<bool> BranchHasPendingTransfers(Guid branchId);
}

public record ProductVariantStockDto(
    Guid Id,
    string Sku,
    decimal Price,
    int Stock);

public record StockDeductionDto(
    Guid ProductVariantId,
    int Quantity);