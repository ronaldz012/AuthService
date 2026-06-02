using Inventory.Contracts.Dtos.Brands;
using Inventory.Data.Entities.Products;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Brands;

public class CreateBrand(InvDbContext context)
{
    public async Task<Result<BrandDto>> Execute(CreateBrandDto dto)
    {
        var uniquePrefix = context.Brands.Any(b => b.Prefix == dto.Prefix);
            if(uniquePrefix) return new Error("DUPLICATE", "there is already a brand with that prefix");
            
        var newBrand = new Brand
        {
            Name = dto.Name,
            Description = dto.Description,
            Prefix = dto.Prefix
        };
        context.Brands.Add(newBrand);
        await context.SaveChangesAsync();
        return new BrandDto
        {
            Id = newBrand.Id,
            Name = newBrand.Name,
            Prefix = newBrand.Prefix,
            Description = newBrand.Description
        };
    }
}