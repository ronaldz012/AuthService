using System.ComponentModel.DataAnnotations;

namespace Inventory.Contracts.Dtos.Receptions;

public class CreateStockReceptionDto
{
    public string? Notes { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<CreateStockReceptionItemDto> Items { get; set; } = new();
}

public class CreateStockReceptionItemDto
{
    [Required]
    public Guid ProductVariantId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "QuantityReceived must be greater than 0")]
    public int QuantityReceived { get; set; }

    [Range(0.5, double.MaxValue, ErrorMessage = "UnitCost must be at least 0.5")]
    public decimal UnitCost { get; set; }
}