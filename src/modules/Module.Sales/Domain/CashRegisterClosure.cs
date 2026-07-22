using Common.Domain;

namespace Module.Sales.Domain;

public class CashRegisterClosure: IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid OpenById { get; set; }
    public string OpenByName { get; set; } = string.Empty;
    public Guid? CloseById { get; set; }
    public string? CloseByName { get; set; }

    public DateTime OpenAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal SystemSalesAmount { get; set; }
    public decimal RealCountedAmount { get; set; }

    public bool IsOpen { get; set; }
    public ICollection<CashRegisterMovement> Movements { get; set; } = new List<CashRegisterMovement>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public static CashRegisterClosure Open(Guid branchId, decimal openingBalance, Guid openById, string openByName)
    {
        return new CashRegisterClosure
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            OpeningBalance = openingBalance,
            OpenById = openById,
            OpenByName = openByName,
            OpenAt = DateTime.UtcNow,
            IsOpen = true
        };
    }

    public void Close(decimal realCountedAmount, Guid closedById, string closedByName)
    {
        if (!IsOpen)
            throw new InvalidOperationException("Cash register is already closed.");

        var cashSalesTotal = Sales
            .Where(s => s.PaymentMethod == PaymentMethod.Cash)
            .Sum(s => s.TotalAmount);

        var outflowsTotal = Movements
            .Where(m => m.Type == CashRegisterMovementType.Outflow)
            .Sum(m => m.Amount);

        SystemSalesAmount = OpeningBalance + cashSalesTotal - outflowsTotal;
        RealCountedAmount = realCountedAmount;
        CloseById = closedById;
        CloseByName = closedByName;
        ClosedAt = DateTime.UtcNow;
        IsOpen = false;
    }
}
