using System.ComponentModel.DataAnnotations;

namespace Module.Inventory.Application.UseCases.Providers.Update;

public class UpdateProviderRequest
{
    [Required, MinLength(2), MaxLength(150)]
    public string? Name { get; set; }

    [MaxLength(150)]
    public string? ContactName { get; set; }

    [EmailAddress, MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }
}
