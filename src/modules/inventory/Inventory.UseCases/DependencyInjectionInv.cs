using Inventory.Contracts.interfaces;
using Inventory.Data.Entities.Transfers;
using Inventory.UseCases.Brands;
using Inventory.UseCases.Categories;
using Inventory.UseCases.Colors;
using Inventory.UseCases.Products;
using Inventory.UseCases.ProductVariants;
using Inventory.UseCases.Receptions;
using Inventory.UseCases.Transfers;
using Microsoft.Extensions.DependencyInjection;
using Inventory.Contracts.Dtos.ProductVariants;

namespace Inventory.UseCases;

public  static class DependencyInjectionInv
{
    public static IServiceCollection AddInventory(this IServiceCollection services)
    {
        services.AddScoped<ProductUseCases>()
            .AddScoped<CreateProductUc>()
            .AddScoped<ListProducts>()
            .AddScoped<SearchProduct>()
            .AddScoped<ValidateProducts>()
            .AddScoped<ValidateProductVariants>()
            .AddScoped<ProductDetails>()
            .AddScoped<GetProductVariantByCode>()
            .AddScoped<UpdateProduct>()
            .AddScoped<DeleteProduct>();

        services.AddScoped<ProductVariantUseCases>()
            .AddScoped<ProductVariantDetailsDto>()
            .AddScoped<UpdateProductVariant>()
            .AddScoped<CorrectProductVariantStock>()
            .AddScoped<CreateProductVariantUc>()
            .AddScoped<DeleteProductVariantUc>();
        
        services.AddScoped<CategoryUseCases>()
            .AddScoped<CreateCategory>()
            .AddScoped<GetCategories>();
        
        services.AddScoped<BrandUseCases>()
            .AddScoped<CreateBrand>()
            .AddScoped<GetBrands>();
        
        services.AddScoped<ReceptionUseCases>()
            .AddScoped<CreateReceptionUc>()
            .AddScoped<ListReceptions>()
            .AddScoped<GetReception>()
            .AddScoped<ReceptionLabels>();

        services.AddScoped<StockTransferUseCases>()
            .AddScoped<CreateStockTransfer>()
            .AddScoped<ResolveStockTransfer>()
            .AddScoped<StockTransferDetails>()
            .AddScoped<CancelStockTransfer>()
            .AddScoped<ListStockTransfers>();

        services.AddScoped<ColoreUseCases>()
            .AddScoped<CreateColor>()
            .AddScoped<GetListColors>();

        services.AddScoped<IInventoryIntegrationService, InventoryIntegrationService>();
        

        return services;
    }
}