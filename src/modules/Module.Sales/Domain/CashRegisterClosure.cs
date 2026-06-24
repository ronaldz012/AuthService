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

        public CashRegisterClosureStatus Status { get; set; }
        public ICollection<CashRegisterMovement> Movements { get; set; } = new List<CashRegisterMovement>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
public enum CashRegisterClosureStatus
{
    Open,
    Closed,
}
