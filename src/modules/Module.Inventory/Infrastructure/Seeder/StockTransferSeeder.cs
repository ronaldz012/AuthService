using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Infrastructure.Seeder;

public class StockTransferSeeder(IServiceProvider serviceProvider) : IDataSeeder
{
    public int Order => 7;

    public async Task SeedAsync()
    {
        var context = serviceProvider.GetRequiredService<IInvDbContext>();

        if (await context.StockTransfers.AnyAsync()) return;

        var tenantInfo = await serviceProvider
            .GetRequiredService<Common.Contracts.authentication.ITenantDatabaseResolver>()
            .GetByDisplayName("default");

        if (tenantInfo is null || tenantInfo.BranchIds.Count < 2) return;

        var fromBranchId = tenantInfo.BranchIds[0];
        var toBranchId = tenantInfo.BranchIds[1];

        var productNames = InventorySeedData.Products.Select(p => p.Name).ToList();

        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .Include(pv => pv.Color)
            .Include(pv => pv.BranchInventories)
            .Where(pv => productNames.Contains(pv.Product.Name))
            .ToListAsync();

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var userId = tenantInfo.OwnerUserId;
            const string userName = "System";

            // Transfer 1: Air Max 90 (Negro/42) — 3 units from Main to Secondary
            var variant1 = variants.FirstOrDefault(pv =>
                pv.Product.Name == "Air Max 90" && pv.Color.Name == "Negro" && pv.Size == "42");

            if (variant1 != null)
            {
                var transfer1 = CreateAndAcceptTransfer(context, variant1.Id, fromBranchId, toBranchId, 3, userId, userName);
                variant1.AddQuantity(-3, fromBranchId, userId, userName);
                variant1.AddQuantity(3, toBranchId, userId, userName);
                var (movOut1, movIn1) = StockMovement.CreateTransfer(fromBranchId, toBranchId, variant1.Id, userId, userName, 3, transfer1.Id);
                context.StockTransfers.Add(transfer1);
                context.StockMovements.Add(movOut1);
                context.StockMovements.Add(movIn1);
            }

            // Transfer 2: Revolution 7 (Rojo/40) — 5 units from Main to Secondary
            var variant2 = variants.FirstOrDefault(pv =>
                pv.Product.Name == "Revolution 7" && pv.Color.Name == "Rojo" && pv.Size == "40");

            if (variant2 != null)
            {
                var transfer2 = CreateAndAcceptTransfer(context, variant2.Id, fromBranchId, toBranchId, 5, userId, userName);
                variant2.AddQuantity(-5, fromBranchId, userId, userName);
                variant2.AddQuantity(5, toBranchId, userId, userName);
                var (movOut2, movIn2) = StockMovement.CreateTransfer(fromBranchId, toBranchId, variant2.Id, userId, userName, 5, transfer2.Id);
                context.StockTransfers.Add(transfer2);
                context.StockMovements.Add(movOut2);
                context.StockMovements.Add(movIn2);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static StockTransfer CreateAndAcceptTransfer(IInvDbContext context, Guid variantId, Guid fromBranchId, Guid toBranchId, int quantity, Guid userId, string userName)
    {
        var transfer = new StockTransfer
        {
            FromBranchId = fromBranchId,
            ToBranchId = toBranchId,
            RequestedByUserId = userId,
            Notes = "Traspaso inicial entre sucursales",
            CreatedBy = userId,
            CreatedByName = userName
        };

        transfer.Items.Add(new StockTransferItem
        {
            ProductVariantId = variantId,
            QuantityRequested = quantity,
            CreatedBy = userId,
            CreatedByName = userName
        });

        transfer.Accept(userId, userName, "Traspaso inicial entre sucursales");
        return transfer;
    }
}
