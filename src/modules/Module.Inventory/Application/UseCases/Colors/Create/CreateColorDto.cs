using System.ComponentModel.DataAnnotations;

namespace Module.Inventory.Application.UseCases.Colors.Create;

public class CreateColorDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;
}