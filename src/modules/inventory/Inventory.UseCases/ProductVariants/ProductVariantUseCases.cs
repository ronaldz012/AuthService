
using Inventory.UseCases.StockMovements;

namespace Inventory.UseCases.ProductVariants;

public record ProductVariantUseCases(
    GetProductVariantDetails GetProductVariantDetails
    ,UpdateProductVariant UpdateProductVariant
    , CorrectProductVariantStock CorrectProductVariantStock
    ,GetProductVariantByCode  GetProductVariantByCode
    ,CreateProductVariantUc CreateProductVariantUc
    ,DeleteProductVariantUc DeleteProductVariantUc
    ,ListStockMovementsUc ListStockMovementsUc);