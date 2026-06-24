using Common.Utilities;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Brands.CreateBrand;

public class CreateBrand(IInvDbContext context)
{
    public async Task<Result<BrandResponse>> Execute(CreateBrandRequest request)
    {
        var uniquePrefix = context.Brands.Any(b => b.Prefix == request.Prefix);
            if(uniquePrefix) return new Error("DUPLICATE", "there is already a brand with that prefix");
            
        var newBrand = new Brand
        {
            Name = request.Name,
            Description = request.Description,
            Prefix = request.Prefix
        };
        context.Brands.Add(newBrand);
        await context.SaveChangesAsync();
        return new BrandResponse
        {
            Id = newBrand.Id,
            Name = newBrand.Name,
            Prefix = newBrand.Prefix,
            Description = newBrand.Description
        };
    }
}