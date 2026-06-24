namespace Module.Inventory.Application.UseCases.ProductVariants.Create;

public class CreateProductVariantDto
{
    public Guid ColorId {get; set;}
    public string Size {get;set;} = string.Empty;
    public decimal Price {get;set; }
    public string Description {get; set; } = string.Empty;

}
public class ProductVariantCreatedDto
{
    public Guid ProductVariantId {get;set;}
    public string Sku {get;set;} = string.Empty;
    public string Size {get;set;} = string.Empty;
    public string ColorName {get;set;} = string.Empty;
}