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

        var productNames = InventorySeedData.Products.Select(p => p.Name).ToList();

        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .Include(pv => pv.Color)
            .Include(pv => pv.BranchInventories)
            .Where(pv => productNames.Contains(pv.Product.Name))
            .ToListAsync();

        var branchId = variants
            .SelectMany(pv => pv.BranchInventories)
            .Select(bi => bi.BranchId)
            .FirstOrDefault();

        if (branchId == Guid.Empty)
        {
            var tenantInfo = await serviceProvider
                .GetRequiredService<Common.Contracts.authentication.ITenantDatabaseResolver>()
                .GetByDisplayName("default");
            branchId = tenantInfo?.MainBranchId ?? Guid.Empty;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var reception = StockReception.Create(branchId, "Stock inicial");
            var stockMovements = new List<StockMovement>();

            foreach (var prodSeed in InventorySeedData.Products)
            {
                foreach (var varSeed in prodSeed.Variants)
                {
                    var variant = variants.FirstOrDefault(pv =>
                        pv.Product.Name == prodSeed.Name &&
                        pv.Color.Name == varSeed.Color &&
                        pv.Size == varSeed.Size);

                    if (variant == null) continue;

                    reception.AddExistingVariant(variant.Id, varSeed.InitialStock, varSeed.UnitCost);
                    variant.AddQuantity(varSeed.InitialStock, branchId);
                    stockMovements.Add(StockMovement.CreateReception(
                        branchId, variant.Id, Guid.Empty, varSeed.InitialStock, reception.Id, "Stock inicial"));
                }
            }

            context.StockReceptions.Add(reception);
            context.StockMovements.AddRange(stockMovements);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
