namespace Inventory.Contracts.Dtos.ProductVariants;

public class UpdateProductVariantDto
{
    public string? Description { get; set; } 
    public string? Size { get; set; } 
    public string? Color { get; set; } 
    public decimal? Price { get; set; }
}