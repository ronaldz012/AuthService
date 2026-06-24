using System.ComponentModel.DataAnnotations;

namespace Inventory.Contracts.Dtos.ProductVariants;

public class UpdateProductVariantStockDto
{
    [Required,  Range(0, int.MaxValue)]
    public int Stock { get; set; }
    [Required,  MinLength(3)]
    public string Notes { get; set; } = string.Empty;
}