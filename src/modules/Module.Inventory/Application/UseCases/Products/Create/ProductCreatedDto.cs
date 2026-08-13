namespace Module.Inventory.Application.UseCases.Products.Create;

public class ProductCreatedDto
{
    public required Guid Id {get ;set;}
    public required string InternalCode {get; set;} = string.Empty;
    public required string Name {get;set;} = string.Empty;
    public required string BrandName  {get;set;}
    public required string CategoryName { get; set; }
    public bool IsActive { get; set; } = true;

    public List<ProductVariantsCreated> Variants { get; set; } = [];


}

public class ProductVariantsCreated
{
    public Guid ProductVariantId {get ;set;}
    public string Sku {get; set;} = string.Empty;
    public string Size {get; set;} = string.Empty;
    public string ColorName {get; set;} = string.Empty;
}
