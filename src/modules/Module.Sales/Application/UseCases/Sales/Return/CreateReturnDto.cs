using System.ComponentModel.DataAnnotations;

namespace Module.Sales.Application.UseCases.Sales.Return;

public class CreateReturnDto : IValidatableObject
{
    [Required]
    public Guid OriginalSaleId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<CreateReturnItemDto> Items { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var duplicate = Items
            .GroupBy(x => x.OriginalSaleItemId)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate != null)
            yield return new ValidationResult("Duplicate items in request", [nameof(Items)]);
    }
}

public class CreateReturnItemDto
{
    [Required]
    public Guid OriginalSaleItemId { get; set; }

    [Range(1, 99999, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}
