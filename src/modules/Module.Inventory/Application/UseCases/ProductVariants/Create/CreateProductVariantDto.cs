using System.ComponentModel.DataAnnotations;

namespace Module.Inventory.Application.UseCases.ProductVariants.Create;

public class CreateProductVariantDto
{
    public Guid ColorId {get; set;}
    public string Size {get;set;} = string.Empty;
    public decimal Price {get;set; }
    public string Description {get; set; } = string.Empty;

}
public class CreateProductVariantsRequest : IValidatableObject
{
    [MinLength(1, ErrorMessage = "The variant list cannot be empty.")]
    public List<CreateProductVariantDto> Variants { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var duplicate = Variants
            .GroupBy(x => new { x.ColorId, Size = x.Size.Trim().ToLower() })
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate != null)
            yield return new ValidationResult("There are duplicate variants (same size and color) in your request.", [nameof(Variants)]);
    }
}
public class ProductVariantCreatedDto
{
    public Guid ProductVariantId {get;set;}
    public string Sku {get;set;} = string.Empty;
    public string Size {get;set;} = string.Empty;
    public string ColorName {get;set;} = string.Empty;
}