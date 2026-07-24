using Common.Contracts.authentication;
using Common.Utilities;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Categories.Create;

public class CreateCategory(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<CategoryCreatedResponse>> Execute(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            CreatedBy = currentUser.UserId,
            CreatedByName = currentUser.FullName
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