using System.ComponentModel.DataAnnotations;
using Inventory.Data.Entities.Products;

namespace Inventory.Contracts.Dtos.Products;

public class UpdateProductDto
{
    [StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }

    [EnumDataType(typeof(Gender), ErrorMessage = "Género no válido")]
    public Gender? Gender { get; set; }
}