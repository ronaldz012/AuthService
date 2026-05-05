
namespace Inventory.UseCases.ProductVariants;

public record ProductVariantUseCases(UpdateProductVariant UpdateProductVariant
    , CorrectProductVariantStock CorrectProductVariantStock
    ,GetProductVariantByCode  GetProductVariantByCode);