using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Common.Utilities;
using Org.BouncyCastle.Crypto.Engines;

namespace Module.Sales.Application.UseCases.Sales.Search;

public class SkuSearchQueryDto : PaginationQueryDto
{
    [Required]
    public string Sku { get; set; } = string.Empty;

    [Range(1, 90)]
    public int Days { get; set; } = 7;
}

public class SaleSkuSearchDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string SoldByName { get; set; } = string.Empty;
    public int TotalItems { get; set; }  //3 Differente Producits
    public decimal TotalUnitsSold { get; set; }   //total numner oof sold Produtcs
    public MatchedItemDto MatchedItems { get; set; } = new();
}

public class MatchedItemDto
{
    public Guid SaleItemId { get; set; }
    public string ProductDisplayName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
