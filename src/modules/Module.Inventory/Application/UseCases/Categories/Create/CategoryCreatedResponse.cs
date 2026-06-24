using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Categories.CreateCategory;

public class CategoryCreatedResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
