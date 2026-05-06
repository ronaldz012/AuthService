using Inventory.Data.Entities.sales;
using Shared.Extensions;

namespace Inventory.Contracts.Dtos.Sales;

public class SaleListDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public SaleStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public int? InvoiceNumber { get; set; }
    public string? TransactionCode { get; set; }
}

public class SalesQueryDto : GenericPaginationQueryDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public SaleStatus? SaleStatus { get; set; }
}