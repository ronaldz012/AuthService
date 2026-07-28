namespace Module.Inventory.Application.Abstraction;

public interface IProductCodeService
{
    Task<string> ReserveBrandCounter(Guid brandId, string prefix);
    Task<string> ReserveVariantCounter(Guid productId, string productCode);
}
