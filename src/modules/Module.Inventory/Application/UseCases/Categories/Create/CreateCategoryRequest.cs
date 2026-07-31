using System.ComponentModel.DataAnnotations;

namespace Module.Inventory.Application.UseCases.Categories.Create;

public class CreateCategoryRequest
{
    [Required, MinLength(2), MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ParentId { get; set; }
}
