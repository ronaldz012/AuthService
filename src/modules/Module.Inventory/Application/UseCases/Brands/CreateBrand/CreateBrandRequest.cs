using System.ComponentModel.DataAnnotations;

namespace Module.Inventory.Application.UseCases.Brands.CreateBrand;

public class CreateBrandRequest
{
    [Required, MinLength(3), MaxLength(15)]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required, MinLength(3), MaxLength(3)]
    public string Prefix {get;set;} = string.Empty;
}