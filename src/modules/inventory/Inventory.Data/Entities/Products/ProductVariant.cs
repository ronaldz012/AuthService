using Inventory.Data.Entities.Inventory;
using Inventory.Data.Entities.Receptions;
using Inventory.Data.Entities.Shared.Base;
using Inventory.Data.Entities.Transfers;

namespace Inventory.Data.Entities.Products;

public class ProductVariant: Params
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; 
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Price { get; set; }

    
    public Product Product { get; set; } = default!;
    public ICollection<BranchInventory> BranchInventories { get; set; } = new List<BranchInventory>();
    public ICollection<StockReceptionItem>  StockReceptionItems { get; set; } = new List<StockReceptionItem>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public ICollection<StockTransferItem> TransferItems { get; set; } = new List<StockTransferItem>();
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
            throw new InvalidOperationException($"Stock insuficiente para {Sku}. Disponible: {branchInventory.Stock}, Solicitado: {quantity}");

        branchInventory.Stock -= quantity;
    }
    public int GetStockByBranch(Guid branchId) 
        => BranchInventories.FirstOrDefault(bi => bi.BranchId == branchId)?.Stock ?? 0;

}
