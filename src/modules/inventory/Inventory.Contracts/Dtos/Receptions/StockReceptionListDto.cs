using Inventory.Data.Entities.Receptions;
using Shared.Extensions;

namespace Inventory.Contracts.Dtos.Receptions;

public class StockReceptionListDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public ReceptionStatus Status { get; set; }
    public int TotalItems { get; set; }
    public int ProductVariantsCount { get; set; }
    public decimal TotalCost { get; set; }
    public List<string> BrandNames { get; set; } = [];
    public List<string> CategoryNames { get; set; } = [];

}

public class ReceptionQueryDto : GenericPaginationQueryDto
{

}