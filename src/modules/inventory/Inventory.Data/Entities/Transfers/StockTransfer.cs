using System.Collections;
using Inventory.Data.Entities.Inventory;
using Inventory.Data.Entities.Shared.Base;

namespace Inventory.Data.Entities.Transfers;

public class StockTransfer : Params
{
    public int Id { get; set; }
    public Guid FromBranchId { get; set; }
    public Guid ToBranchId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Pending;
    public string? Notes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public ICollection<StockMovement> StockMovements { get; set; } = [];
    public ICollection<StockTransferItem> Items { get; set; } = [];
    public void Accept(Guid userId, string? notes)
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException("Only pending transfers can be accepted");

        Status = TransferStatus.Completed;
        ResolvedByUserId = userId;
        ResolvedAt = DateTime.UtcNow;
        if (notes != null) Notes = notes;
    }

    public void Reject(Guid userId, string? notes)
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException("Only pending transfers can be rejected");

        Status = TransferStatus.Rejected;
        ResolvedByUserId = userId;
        ResolvedAt = DateTime.UtcNow;
        if (notes != null) Notes = notes;
    }

    public void Cancel(int userId)
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException("Only pending transfers can be cancelled");

        Status = TransferStatus.Cancelled;
        ResolvedAt = DateTime.UtcNow;
    }
}


public enum TransferStatus
{
    Pending,
    Transit,
    Completed,
    Rejected,
    Cancelled
}