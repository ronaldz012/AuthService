using System.ComponentModel.DataAnnotations;

namespace Module.Auth.Application.UseCases.Features;

public class CreateFeatureDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Route { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Domain.Module Module { get; set; }

}

