using Inventory.Contracts.Dtos.Categories;
using Inventory.Data.Entities.Products;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Categories;

public class CreateCategory(InvDbContext context)
{
    public async Task<Result<CategoryDto>> Execute(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
        };
        context.Add(category);
        await context.SaveChangesAsync();
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
        };
    }
}