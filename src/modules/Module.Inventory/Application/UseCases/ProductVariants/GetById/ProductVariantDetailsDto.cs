namespace Module.Inventory.Application.UseCases.ProductVariants.GetById;

public class ProductVariantDetailsDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory {get; set;} = string.Empty;
    public string ProductBrand {get;set;} = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Size { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal AverageCost { get; set; }

    public decimal CurrentStock { get; set; }
}
