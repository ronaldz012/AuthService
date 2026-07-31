using Common.Domain;
using Module.Inventory.Domain.Receptions;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Organization;

public class Provider : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid TenantId { get; set; }

    public ICollection<StockReception> StockReceptions { get; set; } = new List<StockReception>();

    public static Provider Create(
        string name,
        Guid tenantId,
        Guid createdBy,
        string createdByName,
        string? contactName = null,
        string? email = null,
        string? phoneNumber = null,
        string? address = null)
    {
        return new Provider
        {
            Id = Guid.NewGuid(),
            Name = name,
            ContactName = contactName,
            Email = email,
            PhoneNumber = phoneNumber,
            Address = address,
            IsActive = true,
            TenantId = tenantId,
            CreatedBy = createdBy,
            CreatedByName = createdByName
        };
    }
}
