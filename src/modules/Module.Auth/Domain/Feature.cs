using Module.Auth.Domain;
namespace Module.Auth.Domain;

public class Feature 
{
    public string Key { get; set; } = string.Empty; 
    public string Route { get; set; } = string.Empty;
    public bool IsMenu { get; set; } = false;
    public string DisplayName { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public required Module Module { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public List<FeatureAction> AvailableActions { get; set; } = [];
    
    public ICollection<RoleFeaturePermission> RoleFeaturePermissions { get; set; } = new List<RoleFeaturePermission>();
}
public class FeatureAction
{
    public string Key { get; set; } = string.Empty;         // "update_stock"
    public string DisplayName { get; set; } = string.Empty; // "Actualizar Stock"
    public string Description { get; set; } = string.Empty; // "Permite modificar el inventario de la sucursal"
}
public enum Module
{
    Inventory =0,
    Sales =1
}