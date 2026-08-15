using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Brands.CreateBrand;

public class CreateBrandUc(IInvDbContext context)
{
    public async Task<Result<BrandResponse>> Execute(ActorContext ctx, CreateBrandRequest request)
    {
        var uniquePrefix = await context.Brands.AnyAsync(b => b.Prefix.ToLower() == request.Prefix.ToLower());
            if(uniquePrefix) return CreateBrandErrors.BrandPrefixAlreadyExists;

        var uniqueName = await context.Brands.AnyAsync(b => b.Name.ToLower() == request.Name.ToLower());
            if(uniqueName) return CreateBrandErrors.BrandNameAlreadyExists;
            
        var newBrand = new Brand
        {
            Name = request.Name,
            Description = request.Description,
            Prefix = request.Prefix,
            CreatedBy = ctx.UserId,
            CreatedByName = ctx.FullName
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