using Common.Domain;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Products; 

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

    public int UnitMeasurementSin { get; set; }
    public string EconomicActivity { get; set; } = string.Empty;
    public int ProductCodeSin { get; set; }

    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public Category Category { get; set; } = default!;
    public Brand Brand { get; set; } = null!;

    public static Product Create(string name, string description, Guid categoryId, Guid brandId, Gender gender, string internalCode, Guid tenantId, Guid createdBy, string createdByName)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CategoryId = categoryId,
            BrandId = brandId,
            Gender = gender,
            InternalCode = internalCode,
            TenantId = tenantId,
            ProductVariantCounter = 0,
            CreatedBy = createdBy,
            CreatedByName = createdByName
        };
    }
}

public enum Gender
{
    Unisex,
    Male,
    Female
}
