namespace Module.Inventory.Application.UseCases.ProductVariants.Delete;

public class ProductVariantDeleteCheckDto
{
    public Guid VariantId { get; set; }
    public bool CanDelete { get; set; }
    public string Reason { get; set; } = string.Empty;
}