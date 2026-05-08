using sales.use.Entities;

namespace sales.Contracts.dtos;

public class CreateSaleDto
{
    public Guid BranchId { get; set; }
    public Guid SoldById { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionCode { get; set; }
    public string? Notes { get; set; }
    
    // Lista de productos a vender
    public List<CreateSaleItemDto> Items { get; set; } = new();
}

public class CreateSaleItemDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; } // Agregué cantidad, necesaria para el cálculo
    public decimal DiscountAmount { get; set; }
}