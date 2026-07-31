using Module.Inventory.Domain.Organization;

namespace Module.Inventory.Application.UseCases.Providers.CreateProvider;

public class ProviderResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }

    public static ProviderResponse FromEntity(Provider provider) => new()
    {
        Id = provider.Id,
        Name = provider.Name,
        ContactName = provider.ContactName,
        Email = provider.Email,
        PhoneNumber = provider.PhoneNumber,
        Address = provider.Address,
        IsActive = provider.IsActive
    };
}
