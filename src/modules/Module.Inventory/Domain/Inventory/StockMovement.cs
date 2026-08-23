using Common.Domain;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Shared.Base;

namespace Module.Inventory.Domain.Inventory;

public class StockMovement : Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid UserId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string Notes { get; set; } = string.Empty;
    public MovementType MovementType { get; set; }
    public Guid? TransferToBranchId { get; set; }
    public Guid? ReferenceId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    // Ingreso por recepción
    public static StockMovement CreateReception(Guid branchId, Guid productVariantId, Guid userId, string userName, decimal quantity, Guid referenceId, decimal unitCost, string? notes = null)
    {
        return new StockMovement
        {
            BranchId = branchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = quantity,
            UnitCost = unitCost,
            MovementType = MovementType.Reception,
            Notes = notes ?? string.Empty,
            ReferenceId = referenceId,
            CreatedBy = userId,
            CreatedByName = userName
        };
    }
    public static StockMovement CreateReceptionForNewVariant(Guid branchId, ProductVariant productVariant, Guid userId, string userName, decimal quantity,Guid referenceId, decimal unitCost, string? notes = null)
    {
        return new StockMovement
        {
            BranchId = branchId,
            ProductVariant = productVariant, // EF resuelve el Id
            UserId = userId,
            Quantity = quantity,
            UnitCost = unitCost,
            MovementType = MovementType.Reception,
            Notes = notes ?? string.Empty,
            ReferenceId =  referenceId,
            CreatedBy = userId,
            CreatedByName = userName
            
        };
    }

    // Egreso por venta
    public static StockMovement CreateSale(Guid branchId, Guid productVariantId, Guid userId, string userName, decimal quantity,Guid referenceId, decimal unitCost, string? notes = null)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero");

        return new StockMovement
        {
            BranchId = branchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = -quantity, // negativo representa egreso
            UnitCost = unitCost,
            MovementType = MovementType.Sale,
            Notes = notes ?? string.Empty,
            ReferenceId = referenceId,
            CreatedBy = userId,
            CreatedByName = userName
            
        };
    }

    // Ajuste manual (puede ser positivo o negativo)
    public static StockMovement CreateAdjustment(Guid branchId, Guid productVariantId, Guid userId, string userName, decimal quantity,  string notes, decimal unitCost = 0)
    {
        if (string.IsNullOrEmpty(notes))
            throw new InvalidOperationException("Adjustment requires a note explaining the reason");

        return new StockMovement
        {
            BranchId = branchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = quantity,
            UnitCost = unitCost,
            MovementType = MovementType.Adjustment,
            Notes = notes,
            CreatedBy = userId,
            CreatedByName = userName

        };
    }

    // Egreso por reversión de recepción
    public static StockMovement CreateReceptionRevert(Guid branchId, Guid productVariantId, Guid userId, string userName, decimal quantity, Guid referenceId, decimal unitCost, string? notes = null)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero");

        return new StockMovement
        {
            BranchId = branchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = -quantity,
            UnitCost = unitCost,
            MovementType = MovementType.ReceptionRevert,
            Notes = notes ?? string.Empty,
            ReferenceId = referenceId,
            CreatedBy = userId,
            CreatedByName = userName
        };
    }

    // Traspaso — devuelve los dos movimientos linkeados
    public static (StockMovement Out, StockMovement In) CreateTransfer(
        Guid fromBranchId, Guid toBranchId, Guid productVariantId, Guid userId, string userName, decimal quantity,Guid referenceId, decimal unitCost, string? notes = null)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero");

        var transferOut = new StockMovement
        {
            BranchId = fromBranchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = -quantity,
            UnitCost = unitCost,
            MovementType = MovementType.TransferOut,
            TransferToBranchId = toBranchId,
            Notes = notes ?? string.Empty,
            ReferenceId = referenceId,
            CreatedBy = userId,
            CreatedByName = userName
        };

        var transferIn = new StockMovement
        {
            BranchId = toBranchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = quantity,
            UnitCost = unitCost,
            MovementType = MovementType.TransferIn,
            Notes = notes ?? string.Empty,
            ReferenceId = referenceId,
            CreatedBy = userId,
            CreatedByName = userName
        };
        return (transferOut, transferIn);
    }

    // Ingreso por devolución de venta
    public static StockMovement CreateReturn(
        Guid branchId,
        Guid productVariantId,
        Guid userId,
        string userName,
        decimal quantity,
        Guid referenceId,
        decimal unitCost,
        string? notes = null)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero");

        return new StockMovement
        {
            BranchId = branchId,
            ProductVariantId = productVariantId,
            UserId = userId,
            Quantity = quantity, // POSITIVO - el stock VUELVE
            UnitCost = unitCost, // Costo de la venta ORIGINAL (snapshot)
            MovementType = MovementType.Return,
            Notes = notes ?? string.Empty,
            ReferenceId = referenceId, // Id de la Sale tipo Return
            CreatedBy = userId,
            CreatedByName = userName
        };
    }
}

public enum MovementType
{
    Reception,        // ingreso por recepción de mercadería
    Sale,             // egreso por venta
    Return,           // ingreso por devolución de venta
    Adjustment,       // ajuste manual
    TransferOut,      // egreso por traspaso
    TransferIn,       // ingreso por traspaso
    ReceptionRevert   // egreso por reversión de recepción
}