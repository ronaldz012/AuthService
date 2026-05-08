using Common.Domain;
using Inventory.Data.Entities.Products;
using Inventory.Data.Entities.Shared.Base;
using Inventory.Data.Entities.Transfers;

namespace Inventory.Data.Entities.Inventory;

public class StockMovement : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid UserId { get; set; }
    public decimal Quantity { get; set; }
    public string Notes { get; set; } = string.Empty;
    public MovementType MovementType { get; set; }
    public Guid? TransferToBranchId { get; set; }
    public Guid? stockTransferId { get; set; }

    public StockTransfer? StockTransfer { get; set; }
    //public int? RelatedMovementId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;
    //public StockMovement? RelatedMovement { get; set; }

    // Ingreso por recepción
    public static StockMovement CreateReception(Guid branchId, Guid productVariantId, Guid userId, decimal quantity, string? notes = null)
    {
        return new StockMovement
        {
            BranchId = branchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = quantity,
            MovementType = MovementType.Reception,
            Notes = notes ?? string.Empty
        };
    }
    public static StockMovement CreateReceptionForNewVariant(Guid branchId, ProductVariant productVariant, Guid userId, decimal quantity, string? notes = null)
    {
        return new StockMovement
        {
            BranchId = branchId,
            ProductVariant = productVariant, // EF resuelve el Id
            UserId = userId,
            Quantity = quantity,
            MovementType = MovementType.Reception,
            Notes = notes ?? string.Empty
        };
    }

    // Egreso por venta
    public static StockMovement CreateSale(Guid branchId, Guid productVariantId, Guid userId, decimal quantity, string? notes = null)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero");

        return new StockMovement
        {
            BranchId = branchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = -quantity, // negativo representa egreso
            MovementType = MovementType.Sale,
            Notes = notes ?? string.Empty
        };
    }

    // Ajuste manual (puede ser positivo o negativo)
    public static StockMovement CreateAdjustment(Guid branchId, Guid productVariantId, Guid userId, decimal quantity, string notes)
    {
        if (string.IsNullOrEmpty(notes))
            throw new InvalidOperationException("Adjustment requires a note explaining the reason");

        return new StockMovement
        {
            BranchId = branchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = quantity,
            MovementType = MovementType.Adjustment,
            Notes = notes
        };
    }

    // Traspaso — devuelve los dos movimientos linkeados
    public static (StockMovement Out, StockMovement In) CreateTransfer(
        Guid fromBranchId, Guid toBranchId, Guid productVariantId, Guid userId, decimal quantity, string? notes = null)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero");

        var transferOut = new StockMovement
        {
            BranchId = fromBranchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = -quantity,
            MovementType = MovementType.TransferOut,
            TransferToBranchId = toBranchId,
            Notes = notes ?? string.Empty
        };

        var transferIn = new StockMovement
        {
            BranchId = toBranchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = quantity,
            MovementType = MovementType.TransferIn,
            Notes = notes ?? string.Empty
        };
        return (transferOut, transferIn);
    }
}

public enum MovementType
{
    Reception,   // ingreso por recepción de mercadería
    Sale,        // egreso por venta
    Adjustment,  // ajuste manual
    TransferOut, // egreso por traspaso
    TransferIn   // ingreso por traspaso
}