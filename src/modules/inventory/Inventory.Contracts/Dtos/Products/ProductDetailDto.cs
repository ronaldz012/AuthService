namespace Inventory.Contracts.Dtos.Products;

public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InternalCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string CategoryName  { get; set; } = string.Empty;
    public string BrandName  { get; set; } = string.Empty;
    public int TotalStockInBranches { get; set; }

    public IEnumerable<ProductVariantDetailDto> Variants { get;set;}=[];
    
}

public class ProductVariantDetailDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<StockDto> Stock { get; set; } = [];
    public int StockOfVariantinBranches { get; set; }
}

public class StockDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Stock { get; set; }
}
