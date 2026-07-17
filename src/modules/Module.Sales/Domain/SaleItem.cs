
using Common.Domain;

namespace Module.Sales.Domain;

public class SaleItem : IMustHaveTenant
{
    public Guid Id { get; set; } 
    public Guid TenantId { get; set; }
    public Guid SaleId { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductDisplayName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }

    public Sale Sale { get; set; } = null!;
}