using System.ComponentModel.DataAnnotations;

namespace Module.Inventory.Application.UseCases.Colors.CreateColor;

public class CreateColorDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;
}