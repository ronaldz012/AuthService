using Common.Domain;
using Module.Inventory.Entities.Shared.Base;

namespace Module.Inventory.Entities.Products; 

public class Product:Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InternalCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public Gender Gender { get; set; }
    public decimal BasePrice { get; set; }
    public int ProductVariantCounter {get;set;}

    public int UnitMeasurementSin { get; set; } // unidad de medida siat
    public string EconomicActivity { get; set; } = string.Empty; // codigo actividad economica siat
    public int ProductCodeSin { get; set; } // codigo producto SIN siat

    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public Category Category { get; set; } = default!;
    public Brand Brand { get; set; } = null!;
}

public enum Gender
{
    Unisex,
    Male,
    Female
}

