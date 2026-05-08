using Inventory.Data.Entities.Products;

namespace Inventory.Contracts.Dtos.Products;

public class ProductVariantBySkuDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid BranchId { get; set; }
    public int AvailableStockInBranch { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public Gender Gender { get; set; } 
    public string BranchName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}