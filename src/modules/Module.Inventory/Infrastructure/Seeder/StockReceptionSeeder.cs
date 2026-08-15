using Common.Contracts.authentication;
using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Application.UseCases.Providers.CreateProvider;
using Module.Inventory.Application.UseCases.Receptions.Create;

namespace Module.Inventory.Infrastructure.Seeder;

public class StockReceptionSeeder(IServiceProvider serviceProvider) : IDataSeeder
{
    public int Order => 7;

    public async Task SeedAsync()
    {
        var context = serviceProvider.GetRequiredService<IInvDbContext>();

        if (await context.StockReceptions.AnyAsync()) return;

        var tenantInfo = await serviceProvider
            .GetRequiredService<ITenantDatabaseResolver>()
            .GetByDisplayName("default");

        if (tenantInfo is null || tenantInfo.BranchIds.Count == 0) return;

        var createProvider = serviceProvider.GetRequiredService<CreateProviderUc>();
        var actor = new ActorContext(tenantInfo.TenantId, tenantInfo.OwnerUserId, "System", Guid.Empty, []);

        var providerNames = InventorySeedData.Providers.Select(p => p.Name.ToLower()).ToList();
        var existingProviders = await context.Providers
            .Where(p => providerNames.Contains(p.Name.ToLower()))
            .ToListAsync();

        var providerIds = existingProviders.Select(p => p.Id).ToList();
        foreach (var providerSeed in InventorySeedData.Providers)
        {
            if (await context.Providers.AnyAsync(p => p.Name.ToLower() == providerSeed.Name.ToLower())) continue;

            var providerResult = await createProvider.Execute(actor, new CreateProviderRequest
            {
                Name = providerSeed.Name,
                ContactName = providerSeed.ContactName,
                Email = providerSeed.Email,
                PhoneNumber = providerSeed.PhoneNumber,
                Address = providerSeed.Address
            });

            if (!providerResult.IsSuccess)
                throw new InvalidOperationException(
                    $"Seeding provider {providerSeed.Name} failed: {providerResult.Error?.Code} - {providerResult.Error?.Message}");

            providerIds.Add(providerResult.Value.Id);
        }

        var productNames = InventorySeedData.Products.Select(p => p.Name).ToList();

        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .Include(pv => pv.Color)
            .Include(pv => pv.Size)
            .Where(pv => productNames.Contains(pv.Product.Name))
            .ToListAsync();

        var variantByKey = variants.ToDictionary(
            v => (v.Product.Name, v.Color.Name, v.Size.Name),
            v => v.Id);

        var stockSplit = DistributeStock(InventorySeedData.Products, tenantInfo.BranchIds.Count);
        var createReception = serviceProvider.GetRequiredService<CreateReceptionUc>();

        for (var i = 0; i < tenantInfo.BranchIds.Count; i++)
        {
            var branchId = tenantInfo.BranchIds[i];
            var providerId = providerIds[i % providerIds.Count];

            var dto = new CreateStockReceptionDto
            {
                ProviderId = providerId,
                Notes = "Stock inicial",
                Items = new List<CreateStockReceptionItemDto>()
            };

            foreach (var prodSeed in InventorySeedData.Products)
            {
                foreach (var varSeed in prodSeed.Variants)
                {
                    if (!variantByKey.TryGetValue((prodSeed.Name, varSeed.Color, varSeed.Size), out var variantId))
                        continue;

                    var qty = stockSplit[(prodSeed.Name, varSeed.Color, varSeed.Size)][i];
                    if (qty <= 0) continue;

                    dto.Items.Add(new CreateStockReceptionItemDto
                    {
                        ProductVariantId = variantId,
                        QuantityReceived = qty,
                        UnitCost = varSeed.UnitCost
                    });
                }
            }

            if (dto.Items.Count == 0) continue;

            var branchActor = new ActorContext(tenantInfo.TenantId, tenantInfo.OwnerUserId, "System", branchId, [branchId]);
            var result = await createReception.Execute(branchActor, dto);

            if (!result.IsSuccess)
                throw new InvalidOperationException(
                    $"Seeding reception for branch {branchId} failed: {result.Error?.Code} - {result.Error?.Message}");
        }
    }

    private static Dictionary<(string Product, string Color, string Size), int[]> DistributeStock(
        InventorySeedData.ProductSeed[] products, int branchCount)
    {
        var distribution = new Dictionary<(string, string, string), int[]>();

        foreach (var prodSeed in products)
        {
            foreach (var varSeed in prodSeed.Variants)
            {
                var key = (prodSeed.Name, varSeed.Color, varSeed.Size);
                var total = varSeed.InitialStock;
                var perBranch = new int[branchCount];

                if (branchCount == 1)
                {
                    perBranch[0] = total;
                }
                else
                {
                    var main = (int)(total * 0.6);
                    perBranch[0] = main;

                    var remainder = total - main;
                    for (var i = 1; i < branchCount; i++)
                    {
                        perBranch[i] = remainder / (branchCount - 1);
                    }
                }

                distribution[key] = perBranch;
            }
        }

        return distribution;
    }
}