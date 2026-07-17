using Common.Utilities;
using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Sales.Get;

public class SaleListDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DocumentType DocumentType { get; set; } 
    public int? InvoiceNumber { get; set; } //if the sale is an invoice, this field will contain the invoice number; otherwise, it will be null.
    public string? TransactionCode { get; set; } // QR
    public int ItemCount { get; set; } // Total number of items in the sale
}

public class SalesQueryDto : GenericPaginationQueryDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}