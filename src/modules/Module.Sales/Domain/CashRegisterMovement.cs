using Common.Domain;

namespace Module.Sales.Domain;

public class CashRegisterMovement : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    
    public Guid CashRegisterClosureId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public CashRegisterMovementType Type { get; set; } 

    public CashRegisterClosure CashRegisterClosure { get; set; } = null!;

    public void Update(decimal amount, string description)
    {
        Amount = amount;
        Description = description;
    }

    public static CashRegisterMovement Create(Guid closureId, decimal amount, string description, CashRegisterMovementType type)
    {
        return new CashRegisterMovement
        {
            Id = Guid.NewGuid(),
            CashRegisterClosureId = closureId,
            Amount = amount,
            Description = description,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum CashRegisterMovementType
{
    Outflow,
    Inflow
}