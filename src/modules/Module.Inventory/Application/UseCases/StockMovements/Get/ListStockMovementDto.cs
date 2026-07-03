using Common.Utilities;
using Module.Inventory.Domain.Inventory;

namespace Module.Inventory.Application.UseCases.StockMovements.Get;

public class ListStockMovementDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public MovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string? TransferToBranchName { get; set; }
    public string? ReferenceId { get; set; }
}

public class StockMovementQueryDto : GenericPaginationQueryDto
{
}
