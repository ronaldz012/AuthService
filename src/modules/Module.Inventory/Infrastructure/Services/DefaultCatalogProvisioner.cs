using Common.Contracts.authentication;
using Common.Contracts.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Infrastructure.Services;

public class DefaultCatalogProvisioner(IServiceScopeFactory scopeFactory) : IDefaultCatalogProvisioner
{
    public async Task SeedAsync(Guid tenantId, Guid createdBy, string createdByName, DefaultCatalogTemplate template)
    {
        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var tenantContext = sp.GetRequiredService<ITenantConnectionContext>();
        var resolver = sp.GetRequiredService<ITenantDatabaseResolver>();

        var info = await resolver.GetTenantDatabaseInfo(tenantId);
        if (info is null)
            return;

        tenantContext.TenantId = tenantId;
        tenantContext.DatabaseName = info.DatabaseName;
        tenantContext.Schema = info.Schema;

        var context = sp.GetRequiredService<IInvDbContext>();

        if (await context.Categories.AnyAsync())
            return;

        foreach (var name in template.Colors)
        {
            if (!await context.Colors.AnyAsync(c => c.Name.ToLower() == name.ToLower()))
                context.Colors.Add(Color.Create(name, tenantId, createdBy));
        }

        foreach (var size in template.Sizes)
        {
            if (!await context.Sizes.AnyAsync(s => s.Name.ToLower() == size.Name.ToLower()))
                context.Sizes.Add(Size.Create(size.Name, size.SortOrder, tenantId, createdBy));
        }

        foreach (var brand in template.Brands)
        {
            if (await context.Brands.AnyAsync(b => b.Prefix.ToLower() == brand.Prefix.ToLower()))
                continue;

            var entity = Brand.Create(brand.Name, brand.Prefix, tenantId, createdBy, createdByName);
            entity.Description = brand.Description;
            context.Brands.Add(entity);
        }

        foreach (var category in template.Categories)
        {
            if (!await context.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.ToLower()))
                context.Categories.Add(Category.Create(category.Name, tenantId, createdBy, createdByName));
        }

        await context.SaveChangesAsync();
    }
}