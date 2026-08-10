using System.ComponentModel.DataAnnotations;

namespace Module.Inventory.Application.UseCases.Receptions.Create;

public class CreateStockReceptionDto : IValidatableObject
{
    [Required]
    public Guid ProviderId { get; set; }

    public string? Notes { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<CreateStockReceptionItemDto> Items { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var duplicate = Items
            .GroupBy(x => x.ProductVariantId)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate != null)
            yield return new ValidationResult("Duplicate variant items in request", [nameof(Items)]);
    }
}

public class CreateStockReceptionItemDto
{
    [Required]
    public Guid ProductVariantId { get; set; }

    [Range(1, 999999999, ErrorMessage = "QuantityReceived must be greater than 0")]
    public int QuantityReceived { get; set; }

    [Range(0.5, 999999999, ErrorMessage = "UnitCost must be at least 0.5")]
    public decimal UnitCost { get; set; }
}