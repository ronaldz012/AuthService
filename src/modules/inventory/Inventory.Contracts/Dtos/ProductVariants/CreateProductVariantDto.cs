namespace Inventory.Contracts.Dtos.ProductVariants;

public class CreateProductVariantDto
{
    public Guid ColorId {get; set;}
    public string Size {get;set;} = string.Empty;
    public decimal Price {get;set; }
    public string Description {get; set; } = string.Empty;

}