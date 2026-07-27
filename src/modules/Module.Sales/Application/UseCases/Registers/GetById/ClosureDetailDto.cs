namespace Module.Sales.Application.UseCases.Registers.GetById;

public class ClosureDetailDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string OpenedByName { get; set; } = string.Empty;
    public string? ClosedByName { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal SystemSalesAmount { get; set; }
    public decimal RealCountedAmount { get; set; }
    public decimal Difference { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }
    public decimal TotalExpenses { get; set; }
    public List<ClosureSaleItemDto> Sales { get; set; } = [];
    public List<ClosureMovementDto> Movements { get; set; } = [];
    public List<ClosureVariantStockDto> VariantStocks { get; set; } = [];
}

public class ClosureSaleItemDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SoldByName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public int? InvoiceNumber { get; set; }
    public string? TransactionCode { get; set; }
    public int ItemsCount { get; set; }
    public List<ClosureSaleItemDetailDto> Items { get; set; } = [];
}

public class ClosureSaleItemDetailDto
{
    public Guid ProductVariantId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductDisplayName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
}

public class ClosureVariantStockDto
{
    public Guid ProductVariantId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductDisplayName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
}

public class ClosureMovementDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
