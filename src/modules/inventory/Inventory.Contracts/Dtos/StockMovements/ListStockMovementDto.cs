using Common.Extensions;
using Inventory.Data.Entities.Inventory;

namespace Inventory.Contracts.Dtos.StockMovements;

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

    public Guid? StockTransferId { get; set; }
}

public class StockMovementsQuery : GenericPaginationQueryDto
{
    
}