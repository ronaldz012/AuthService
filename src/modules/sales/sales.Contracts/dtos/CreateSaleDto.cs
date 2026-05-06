using Inventory.Data.Entities.sales;

namespace Inventory.Contracts.Dtos.Sales;

public class CreateSaleDto
{
    public int BranchId { get; set; }
    public int SoldById { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionCode { get; set; }
    public string? Notes { get; set; }
    
    // Lista de productos a vender
    public List<CreateSaleItemDto> Items { get; set; } = new();
}

public class CreateSaleItemDto
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; } // Agregué cantidad, necesaria para el cálculo
    public decimal DiscountAmount { get; set; }
}