namespace Module.Inventory.Application.UseCases.Products.Create;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public Gender Gender { get; set; }

    public int UnitMeasurementSin { get; set; } // unidad de medida siat
    public string EconomicActivity { get; set; } = string.Empty; // codigo actividad economica siat
    public int ProductCodeSin { get; set; } // codigo producto SIN siat
    public IEnumerable<CreateProductVariantDto> Variants { get;set; } = [];
}


