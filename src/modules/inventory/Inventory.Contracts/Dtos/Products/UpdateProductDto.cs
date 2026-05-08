using System.ComponentModel.DataAnnotations;
using Inventory.Data.Entities.Products;

namespace Inventory.Contracts.Dtos.Products;

public class UpdateProductDto
{
    [StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal? BasePrice { get; set; }

    [Range(1, int.MaxValue)]
    public Guid? CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public Guid? BrandId { get; set; }

    [EnumDataType(typeof(Gender), ErrorMessage = "Género no válido")]
    public Gender? Gender { get; set; }
}