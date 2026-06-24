using System.ComponentModel.DataAnnotations;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Products.Update;

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