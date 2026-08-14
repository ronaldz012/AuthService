namespace Common.Contracts.inventory;

public interface IDefaultCatalogProvisioner
{
    Task SeedAsync(Guid tenantId, Guid createdBy, string createdByName, DefaultCatalogTemplate template);
}