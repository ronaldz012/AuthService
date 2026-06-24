using Common.Utilities;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Products.Get;

public class ListProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InternalCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty; // Para el chip gris
    public string BrandName { get; set; } = string.Empty;    // Para el título en negrita

    // Resúmenes para la metadata secundaria
    public int VariantsCount { get; set; }
    public decimal TotalStock { get; set; }
    public decimal BasePrice { get; set; } // Puede ser el BasePrice o el Min(Price) de variantes
}
public class ProductQueryDto : GenericPaginationQueryDto
{
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Gender? Gender { get; set; }
    public bool? LowStock { get; set; }

}