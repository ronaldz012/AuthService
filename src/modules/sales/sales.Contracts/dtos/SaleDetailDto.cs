using sales.use.Entities;

namespace sales.Contracts.dtos;



public class SaleDetailDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int SoldById { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionCode { get; set; }
    public decimal TotalAmount { get; set; }
    public int? InvoiceNumber { get; set; }
    public SaleStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Detalle de los artículos vendidos
    public List<SaleItemDetailDto> Items { get; set; } = [];
}

public class SaleItemDetailDto
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string? ProductDisplayName { get; set; } 
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
}