using Common.Domain;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Organization;

public class Provider : Params, IMustHaveTenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Nombre del proveedor
    public string ContactName { get; set; } = string.Empty; // Nombre de contacto del proveedor
    public string Email { get; set; } = string.Empty; // Correo electrónico del proveedor
    public string PhoneNumber { get; set; } = string.Empty; // Número de teléfono del proveedor
    public string Address { get; set; } = string.Empty; // Dirección del proveedor
    public Guid TenantId { get; set; }
}
