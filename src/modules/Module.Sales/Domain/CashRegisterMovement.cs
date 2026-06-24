using Common.Domain;

namespace sales.Module.Entities;

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
}

public enum CashRegisterMovementType
{
    Outflow
}