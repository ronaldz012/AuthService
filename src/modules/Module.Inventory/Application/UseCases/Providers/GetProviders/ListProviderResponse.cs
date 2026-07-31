namespace Module.Inventory.Application.UseCases.Providers.GetProviders;

public class ListProviderResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}
