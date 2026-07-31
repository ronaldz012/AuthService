using System.ComponentModel.DataAnnotations;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Products.Create;

public class CreateProductRequest : IValidatableObject
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public Gender Gender { get; set; }

    public int UnitMeasurementSin { get; set; } // unidad de medida siat
    public string EconomicActivity { get; set; } = string.Empty; // codigo actividad economica siat
    public int ProductCodeSin { get; set; } // codigo producto SIN siat
    public IEnumerable<CreateProductVariantForProductDto> Variants { get;set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var duplicate = Variants
            .GroupBy(x => new { x.ColorId, Size = x.Size.Trim().ToLower() })
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate != null)
            yield return new ValidationResult("Duplicate variant combinations (color + size) in request", [nameof(Variants)]);
    }
}


public class CreateProductVariantForProductDto
{
    public Guid ColorId {get; set;}
    public string Size {get;set;} = string.Empty;
    public decimal Price {get;set; }
    public string Description {get; set; } = string.Empty;

}