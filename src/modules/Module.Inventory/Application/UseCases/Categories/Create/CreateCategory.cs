using Common.Utilities;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Categories.CreateCategory;

public class CreateCategory(IInvDbContext context)
{
    public async Task<Result<CategoryCreatedResponse>> Execute(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
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