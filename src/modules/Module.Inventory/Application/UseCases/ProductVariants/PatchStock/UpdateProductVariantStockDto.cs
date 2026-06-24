using System.ComponentModel.DataAnnotations;

namespace Module.Inventory.Application.UseCases.ProductVariants.PatchStock;

public class UpdateProductVariantStockDto
{
    [Required,  Range(0, 999999999)]
    public int Stock { get; set; }
    [Required,  MinLength(3)]
    public string Notes { get; set; } = string.Empty;
}