using System.ComponentModel.DataAnnotations;
using shared.Data.Entities; 
namespace shared.Contracts.dtos.Features;

public class CreateFeatureDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Route { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Module Module { get; set; }

}

