namespace Module.Inventory.Application.UseCases.Categories.CreateCategory;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ParentId { get; set; }
}