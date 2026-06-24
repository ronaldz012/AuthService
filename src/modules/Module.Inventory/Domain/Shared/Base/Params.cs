namespace Module.Inventory.Entities.Shared.Base;

public class Params
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = null;
    public DateTime? DeletedAt { get; set; } = null;
    public Guid? CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
    public Guid? DeletedById { get; set; }
}
