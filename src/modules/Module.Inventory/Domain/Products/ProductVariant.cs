using Common.Domain;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Receptions;
using Module.Inventory.Domain.Shared.Base;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Domain.Products;

public class ProductVariant: Params, IMustHaveTenant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; 
    public string Size { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid TenantId { get; set; }
    public Guid ColorId { get; set; }

    public Color Color { get; set; } = null;
    
    public Product Product { get; set; } = default!;
    public ICollection<BranchInventory> BranchInventories { get; set; } = new List<BranchInventory>();
    public ICollection<StockReceptionItem>  StockReceptionItems { get; set; } = new List<StockReceptionItem>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public ICollection<StockTransferItem> TransferItems { get; set; } = new List<StockTransferItem>();

    public static  string GenerateSku(string internalCode, string colorCode, string size)
    {
        return internalCode +"-"+colorCode+"-"+size;
    }
    public bool HasSufficientStock(int quantity, Guid branchId)
    {
        var branchInventory = BranchInventories.FirstOrDefault(bi => bi.BranchId == branchId);
        return branchInventory != null && branchInventory.Stock >= quantity;
    }

    // DOER: Realiza la acción. Asume que quien lo llama ya validó el estado.
    public void SellStock(int quantity, Guid branchId, Guid userId, Guid referenceId, string? notes = null)
    {
        var branchInventory = BranchInventories.FirstOrDefault(bi => bi.BranchId == branchId);
        if (branchInventory == null)
            throw new InvalidOperationException($"No existe registro de inventario para la sucursal {branchId}");

        if (branchInventory.Stock < quantity)
            throw new InvalidOperationException($"Stock insuficiente para {Sku}"); // Solo actúa como salvaguarda

        branchInventory.Stock -= quantity;
        StockMovements.Add(StockMovement.CreateSale(branchId, Id, userId, quantity, referenceId, notes));
    }


    public void AddQuantity(int quantity, Guid branchId)
    {
        var branchInventory = BranchInventories.FirstOrDefault(bi => bi.BranchId == branchId);
        if (branchInventory == null)
        {
            branchInventory = new BranchInventory
            {
                BranchId = branchId,
                ProductVariantId = Id,
                Stock = 0
            };
            BranchInventories.Add(branchInventory);
        }
        branchInventory.Stock += quantity;
    }

    public void CorrectQuantity(int quantity, Guid branchId)
    {
        var branchInventory = BranchInventories.FirstOrDefault(bi => bi.BranchId == branchId);
        if (branchInventory == null)
        {
            branchInventory = new BranchInventory
            {
                BranchId = branchId,
                ProductVariantId = Id,
                Stock = quantity,
            };
            BranchInventories.Add(branchInventory);
        }
        branchInventory.Stock = quantity;
    }
    public void RemoveQuantity(int quantity, Guid branchId)
    {
        var branchInventory = BranchInventories.FirstOrDefault(bi => bi.BranchId == branchId);
        
        if (branchInventory == null)
            throw new InvalidOperationException($"No existe registro de inventario para la sucursal {branchId}");

        if (branchInventory.Stock < quantity)
            throw new InvalidOperationException($"Cantidad solicitada para {Sku} excede el stock disponible ({branchInventory.Stock})");

        branchInventory.Stock -= quantity;
    }
    public int GetStockByBranch(Guid branchId) 
        => BranchInventories.FirstOrDefault(bi => bi.BranchId == branchId)?.Stock ?? 0;

    public void SoftDelete(Guid userId)
    {
       DeletedAt = DateTime.UtcNow;
       DeletedById = userId;
    }
}
