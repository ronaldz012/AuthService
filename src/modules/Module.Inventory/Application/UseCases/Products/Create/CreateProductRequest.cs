using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Products.Create;

public class CreateProductRequest
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
}


public class CreateProductVariantForProductDto
{
    public Guid ColorId {get; set;}
    public string Size {get;set;} = string.Empty;
    public decimal Price {get;set; }
    public string Description {get; set; } = string.Empty;

}