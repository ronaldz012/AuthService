using System.Drawing;

namespace Inventory.Contracts.Dtos.ProductVariants;

public class ProductVariantCreatedDto
{
    public Guid ProductVariantId {get;set;}
    public string Sku {get;set;} = string.Empty;
    public string Size {get;set;} = string.Empty;
    public string ColorName {get;set;} = string.Empty;
}