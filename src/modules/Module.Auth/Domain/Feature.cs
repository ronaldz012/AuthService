using Module.Auth.Domain;
namespace Module.Auth.Domain;

public class Feature 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public  required Module Module { get; set; }

    //Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
}