using Common.Utilities;
using sales.Module.Entities;

namespace sales.Contracts.dtos;

public class SaleListDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public SaleStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public int? InvoiceNumber { get; set; }
    public string? TransactionCode { get; set; }
}

public class SalesQueryDto : GenericPaginationQueryDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public SaleStatus? SaleStatus { get; set; }
}