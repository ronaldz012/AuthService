using System;
using Shared.Domain;

namespace Auth.Data.Entities;

public class Menu : ICreatedAt, IUpdatedAt
{
    public int Id { get; set; }
    public int ParentMenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Order { get; set; }

    //Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public int ModuleId { get; set; }
    public Module Module { get; set; } = default!;
    
}
