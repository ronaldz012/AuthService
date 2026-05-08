using Inventory.Contracts.Dtos.Brands;
using Inventory.Data.Entities.Products;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Brands;

public class CreateBrand(InvDbContext context)
{
    public async Task<Result<BrandDto>> Execute(CreateBrandDto dto)
    {
        var newBrand = new Brand
        {
            Name = dto.Name,
            Description = dto.Description
        };
        context.Brands.Add(newBrand);
        await context.SaveChangesAsync();
        return new BrandDto
        {
            Id = newBrand.Id,
            Name = newBrand.Name,
            Description = newBrand.Description
        };
    }
}