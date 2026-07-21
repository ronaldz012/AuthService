using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Products.GetById;

public class ProductDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InternalCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public Gender Gender { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int TotalAvailable { get; set; }

    public IEnumerable<ProductVarianListDto> Variants { get; set; } = [];

}

public class ProductVarianListDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public Guid ColorId {get;set;}
    public decimal Price { get; set; }
    public List<BranchStockDto> BranchStocks { get; set; } = [];
    public int TotalAvailable => BranchStocks.Sum(b => b.Stock);
}

public class BranchStockDto
{
    public Guid BranchId { get; set; }
    public int Stock { get; set; }
}

