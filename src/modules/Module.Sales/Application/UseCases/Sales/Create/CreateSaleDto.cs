using Module.Sales.Domain;

namespace Module.Sales.Application.UseCases.Sales.Create;

public class CreateSaleDto
{

    public PaymentMethod PaymentMethod { get; set; }
    public int? InvoiceNumber { get; set; }
    public DocumentType DocumentType { get; set; }
    public Guid CashRegisterClosureId { get; set; }
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