using System.ComponentModel.DataAnnotations;

namespace Module.Inventory.Application.UseCases.Sizes.Create;

public class CreateSizeDto
{
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}