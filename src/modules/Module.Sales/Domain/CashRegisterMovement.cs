using Common.Domain;

namespace Module.Sales.Domain;

public class CashRegisterMovement : IMustHaveTenant, ICreatedAt, ICreatedBy, IUpdatedAt, IUpdatedBy, ISoftDelete
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    
    public Guid CashRegisterClosureId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public string? DeletedByName { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public CashRegisterMovementType Type { get; set; } 

    public CashRegisterClosure CashRegisterClosure { get; set; } = null!;

    public void Update(decimal amount, string description, Guid userId, string userName)
    {
        Amount = amount;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
        UpdatedByName = userName;
    }

    public static CashRegisterMovement Create(Guid closureId, decimal amount, string description, CashRegisterMovementType type, Guid userId, string userName)
    {
        var now = DateTime.UtcNow;
        return new CashRegisterMovement
        {
            Id = Guid.NewGuid(),
            CashRegisterClosureId = closureId,
            Amount = amount,
            Description = description,
            Type = type,
            CreatedAt = now,
            CreatedBy = userId,
            CreatedByName = userName
        };
    }
}

public enum CashRegisterMovementType
{
    Outflow,
    Inflow
}