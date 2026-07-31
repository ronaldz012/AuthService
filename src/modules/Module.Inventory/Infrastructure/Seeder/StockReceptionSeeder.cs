using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Receptions;

namespace Module.Inventory.Infrastructure.Seeder;

public class StockReceptionSeeder(IServiceProvider serviceProvider) : IDataSeeder
{
    public int Order => 6;

    public async Task SeedAsync()
    {
        var context = serviceProvider.GetRequiredService<IInvDbContext>();

        if (await context.StockReceptions.AnyAsync()) return;

        var tenantInfo = await serviceProvider
            .GetRequiredService<Common.Contracts.authentication.ITenantDatabaseResolver>()
            .GetByDisplayName("default");

        if (tenantInfo is null || tenantInfo.BranchIds.Count == 0) return;

        var productNames = InventorySeedData.Products.Select(p => p.Name).ToList();

        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .Include(pv => pv.Color)
            .Where(pv => productNames.Contains(pv.Product.Name))
            .ToListAsync();

        var branchIds = tenantInfo.BranchIds;
        var stockSplit = DistributeStock(InventorySeedData.Products, branchIds.Count);

        var providerNames = InventorySeedData.Providers.Select(p => p.Name).ToList();
        var existingProviders = await context.Providers
            .Where(p => providerNames.Contains(p.Name))
            .ToListAsync();

        var providers = existingProviders.ToList();
        foreach (var providerSeed in InventorySeedData.Providers)
        {
            if (providers.Any(p => p.Name == providerSeed.Name)) continue;

            var provider = Module.Inventory.Domain.Organization.Provider.Create(
                providerSeed.Name,
                tenantInfo.TenantId,
                tenantInfo.OwnerUserId,
                "System",
                providerSeed.ContactName,
                providerSeed.Email,
                providerSeed.PhoneNumber,
                providerSeed.Address);
            context.Providers.Add(provider);
            providers.Add(provider);
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var allMovements = new List<StockMovement>();

            for (var i = 0; i < branchIds.Count; i++)
            {
                var branchId = branchIds[i];
                var provider = providers.Count > 0 ? providers[i % providers.Count] : null;
                var reception = StockReception.Create(branchId, tenantInfo.OwnerUserId, "System", "Stock inicial", provider?.Id);

                foreach (var prodSeed in InventorySeedData.Products)
                {
                    foreach (var varSeed in prodSeed.Variants)
                    {
                        var variant = variants.FirstOrDefault(pv =>
                            pv.Product.Name == prodSeed.Name &&
                            pv.Color.Name == varSeed.Color &&
                            pv.Size == varSeed.Size);

                        if (variant == null) continue;

                        var qty = stockSplit[(prodSeed.Name, varSeed.Color, varSeed.Size)][i];
                        if (qty <= 0) continue;

                        reception.AddExistingVariant(variant.Id, tenantInfo.OwnerUserId, "System", qty, varSeed.UnitCost);
                        variant.AddQuantity(qty, branchId, tenantInfo.OwnerUserId, "System");
                        allMovements.Add(StockMovement.CreateReception(
                            branchId, variant.Id, tenantInfo.OwnerUserId, "System", qty, reception.Id, "Stock inicial"));
                    }
                }

                context.StockReceptions.Add(reception);
            }

            context.StockMovements.AddRange(  );
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
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
