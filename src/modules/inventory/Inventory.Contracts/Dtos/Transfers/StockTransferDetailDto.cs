using Inventory.Data.Entities.Transfers;

namespace Inventory.Contracts.Dtos.Transfers;

public class StockTransferDetailDto
{
    public int Id { get; set; }
    public string FromBranchName { get; set; } = string.Empty;
    public string ToBranchName { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string? ResolverName { get; set; } = null;
    public TransferStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<StockTransferItemDetailDto> Items { get; set; } = [];
}

public class StockTransferItemDetailDto
{
    public int ProductVariantId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string VariantDescription { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int QuantityRequested { get; set; }
}