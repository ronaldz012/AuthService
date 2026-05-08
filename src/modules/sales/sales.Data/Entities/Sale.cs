namespace sales.use.Entities;

public class Sale
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid SoldById { get; set; }
    public string Xd { get; set; } = "xd";
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionCode { get; set; }
    public decimal TotalAmount { get; set; }
    public int? InvoiceNumber { get; set; }
    public SaleStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    
    
}


public enum PaymentMethod
{
    Cash,
    QrCode,
}

public enum SaleStatus
{
    Completed,
    PendingInvoice,
    Cancelled,
}