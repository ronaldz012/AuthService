using Shared.Result;

namespace Inventory.Contracts.interfaces;

public interface IInventoryIntegrationService
{
    Task<Result<List<ProductVariantStockDto>>> GetVariantsWithStock(
        List<int> variantIds, int branchId);

    Task<Result<bool>> DeductStock(
        List<StockDeductionDto> deductions, int branchId, int userId);
}

public record ProductVariantStockDto(
    int Id,
    string Sku,
    decimal Price,
    int Stock);

public record StockDeductionDto(
    int ProductVariantId,
    int Quantity);