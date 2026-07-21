using Common.Utilities;
using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Sales.Get;

public class SaleListDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string SoldByName { get; set; } = string.Empty;
    public string FirstItemDisplayName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DocumentType DocumentType { get; set; } 
    public int? InvoiceNumber { get; set; }
    public string? TransactionCode { get; set; }
}

public class SalesQueryDto : GenericPaginationQueryDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}