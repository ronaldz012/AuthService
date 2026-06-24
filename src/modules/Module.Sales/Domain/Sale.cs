using Common.Domain;

namespace sales.Module.Entities;

public class Sale : IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid SoldById { get; set; }
    public Guid CashRegisterClosureId { get; set; } 
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionCode { get; set; }
    public decimal TotalAmount { get; set; }
    public int? InvoiceNumber { get; set; }
    public SaleType Type { get; set; }        
    public DocumentType DocumentType { get; set; }
    public SaleStatus Status { get; set; }    
    public DateTime? CancelledAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public CashRegisterClosure CashRegisterClosure { get; set; } = null!;


    public static Sale CreateSaleWithTicket(
        Guid branchId,
        Guid soldById,
        Guid cashRegisterClosureId,
        PaymentMethod paymentMethod,
        string? transactionCode,
        string? notes,
        List<(Guid ProductVariantId, decimal UnitPrice, int Quantity, decimal DiscountAmount)> items)
    {
        return CreateBase(branchId, soldById, cashRegisterClosureId, paymentMethod, 
            DocumentType.Ticket, transactionCode, notes, null, items);
    }

    public static Sale CreateSaleWithInvoice(
        Guid branchId,
        Guid soldById,
        Guid cashRegisterClosureId,
        PaymentMethod paymentMethod,
        DocumentType documentType, 
        int? invoiceNumber,
        string? transactionCode,
        string? notes,
        List<(Guid ProductVariantId, decimal UnitPrice, int Quantity, decimal DiscountAmount)> items)
    {
        if (documentType == DocumentType.Ticket)
            throw new InvalidOperationException("Cannot create an invoice with document type 'Ticket'.");

        if (documentType == DocumentType.Invoice && (!invoiceNumber.HasValue || invoiceNumber <= 0))
            throw new InvalidOperationException("Invoice number is required for completed invoices.");

        return CreateBase(branchId, soldById, cashRegisterClosureId, paymentMethod, 
            documentType, transactionCode, notes, invoiceNumber, items);
    }

    // --- Private method to centralize common construction logic ---
    private static Sale CreateBase(
        Guid branchId,
        Guid soldById,
        Guid cashRegisterClosureId,
        PaymentMethod paymentMethod,
        DocumentType documentType,
        string? transactionCode,
        string? notes,
        int? invoiceNumber,
        List<(Guid ProductVariantId, decimal UnitPrice, int Quantity, decimal DiscountAmount)> items)
    {
        if (items == null || !items.Any())
            throw new InvalidOperationException("Cannot create a sale without products.");

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            SoldById = soldById,
            CashRegisterClosureId = cashRegisterClosureId,
            PaymentMethod = paymentMethod,
            DocumentType = documentType,
            Type = SaleType.Sale, 
            TransactionCode = transactionCode,
            Notes = notes,
            InvoiceNumber = invoiceNumber,
            Status = SaleStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        decimal total = 0;
        foreach (var item in items)
        {
            var subtotal = (item.UnitPrice - item.DiscountAmount) * item.Quantity;
            sale.SaleItems.Add(new SaleItem
            {
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                FinalPrice = subtotal
            });
            total += subtotal;
        }

        sale.TotalAmount = total;
        return sale;
    }
    
}

public enum PaymentMethod
{
    Cash,
    QrCode,
}

public enum SaleType
{
    Sale,          // Transacción normal de salida de mercancía
    Return         // Transacción de devolución para la entrada de mercancía
}

public enum DocumentType
{
    Ticket,         // Solo boleta
    Invoice,        // Facturado
    PendingInvoice  // Con factura pendiente en caso de que salga mal
}

public enum SaleStatus
{
    Completed,
    Cancelled,
}