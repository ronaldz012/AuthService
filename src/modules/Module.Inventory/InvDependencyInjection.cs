using Common.Contracts.inventory;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Application.UseCases.Brands;
using Module.Inventory.Application.UseCases.Brands.CreateBrand;
using Module.Inventory.Application.UseCases.Brands.GetBrands;
using Module.Inventory.Application.UseCases.Categories;
using Module.Inventory.Application.UseCases.Categories.Create;
using Module.Inventory.Application.UseCases.Categories.Get;
using Module.Inventory.Application.UseCases.Colors;
using Module.Inventory.Application.UseCases.Colors.Create;
using Module.Inventory.Application.UseCases.Colors.List;
using Module.Inventory.Application.UseCases.Products;
using Module.Inventory.Application.UseCases.Products.Create;
using Module.Inventory.Application.UseCases.Products.Delete;
using Module.Inventory.Application.UseCases.Products.Get;
using Module.Inventory.Application.UseCases.Products.GetById;
using Module.Inventory.Application.UseCases.Products.Search;
using Module.Inventory.Application.UseCases.Products.Update;
using Module.Inventory.Application.UseCases.ProductVariants;
using Module.Inventory.Application.UseCases.ProductVariants.Create;
using Module.Inventory.Application.UseCases.ProductVariants.Delete;
using Module.Inventory.Application.UseCases.ProductVariants.GetById;
using Module.Inventory.Application.UseCases.ProductVariants.GetBySku;
using Module.Inventory.Application.UseCases.ProductVariants.PatchStock;
using Module.Inventory.Application.UseCases.ProductVariants.Update;
using Module.Inventory.Application.UseCases.Receptions;
using Module.Inventory.Application.UseCases.Receptions.Create;
using Module.Inventory.Application.UseCases.Receptions.Get;
using Module.Inventory.Application.UseCases.Receptions.GetById;
using Module.Inventory.Application.UseCases.Receptions.GetLabels;
using Module.Inventory.Application.UseCases.Transfers;
using Module.Inventory.Application.UseCases.Transfers.Cancel;
using Module.Inventory.Application.UseCases.Transfers.Create;
using Module.Inventory.Application.UseCases.Transfers.Get;
using Module.Inventory.Application.UseCases.Transfers.GetById;
using Module.Inventory.Application.UseCases.Transfers.Resolve;
using Module.Inventory.Infrastructure;

namespace Module.Inventory;

public  static class InvDependencyInjection
{
    public static IServiceCollection AddInventory(this IServiceCollection services)
    {
        services.AddScoped<ProductUseCases>()
            .AddScoped<CreateProductUc>()
            .AddScoped<GetProductsUc>()
            .AddScoped<SearchProduct>()
            .AddScoped<ProductDetails>()
            .AddScoped<GetProductVariantByCode>()
            .AddScoped<UpdateProduct>()
            .AddScoped<DeleteProduct>();

        services.AddScoped<ProductVariantUseCases>()
            .AddScoped<GetProductVariantDetails>()
            .AddScoped<GetProductVariantDetails>()
            .AddScoped<UpdateProductVariant>()
            .AddScoped<CorrectProductVariantStock>()
            .AddScoped<CreateProductVariantUc>()
            .AddScoped<DeleteProductVariantUc>();

      
        
        services.AddScoped<CategoryUseCases>()
            .AddScoped<CreateCategory>()
            .AddScoped<GetCategories>();
        
        services.AddScoped<BrandUseCases>()
            .AddScoped<CreateBrandUc>()
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
        services.AddScoped<IInvDbContext>(provider => provider.GetRequiredService<InvDbContext>());
        

        return services;
    }
}