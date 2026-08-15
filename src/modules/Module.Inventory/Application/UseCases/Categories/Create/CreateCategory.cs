using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Categories.Create;

public class CreateCategory(IInvDbContext context)
{
    public async Task<Result<CategoryCreatedResponse>> Execute(ActorContext ctx, CreateCategoryRequest request)
    {
        var existing = await context.Categories
            .AnyAsync(x => x.Name.ToLower() == request.Name.ToLower());

        if (existing)
            return CreateCategoryErrors.CategoryAlreadyExists;

        var category = new Category
        {
            Name = request.Name,
            CreatedBy = ctx.UserId,
            CreatedByName = ctx.FullName
        };
        context.Add(category);
        await context.SaveChangesAsync();
        return new CategoryCreatedResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
        };
    }
}