using Common.Result;

namespace Inventory.Contracts.interfaces;

public interface IInventoryIntegrationService
{
    Task<Result<List<ProductVariantStockDto>>> GetVariantsWithStock(
        List<Guid> variantIds, Guid branchId);

    Task<Result<bool>> DeductStock(
        List<StockDeductionDto> deductions, Guid branchId, Guid userId, Guid referenceId);
}

public record ProductVariantStockDto(
    Guid Id,
    string Sku,
    decimal Price,
    int Stock);

public record StockDeductionDto(
    Guid ProductVariantId,
    int Quantity);