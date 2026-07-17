using Common.Domain;

namespace Module.Sales.Domain;

public class CashRegisterClosure: IMustHaveTenant
{
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid BranchId { get; set; } 
        public Guid OpenById { get; set; }
        public Guid? CloseById { get; set; }

        public DateTime OpenAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public decimal OpeningBalance { get; set; } // Monto inicial
        public decimal SystemSalesAmount { get; set; } // Lo que el sistema calculó
        public decimal RealCountedAmount { get; set; } // Lo que el usuario contó

    public bool IsOpen { get; set; }
    public ICollection<CashRegisterMovement> Movements { get; set; } = new List<CashRegisterMovement>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public void Close(decimal realCountedAmount, Guid closedById)
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
        ClosedAt = DateTime.UtcNow;
        IsOpen = false;
    }
}
