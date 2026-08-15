using Common.Contracts.authentication;
using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Application.UseCases.Transfers.Create;
using Module.Inventory.Application.UseCases.Transfers.Resolve;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Infrastructure.Seeder;

public class StockTransferSeeder(IServiceProvider serviceProvider) : IDataSeeder
{
    public int Order => 8;

    public async Task SeedAsync()
    {
        var context = serviceProvider.GetRequiredService<IInvDbContext>();

        if (await context.StockTransfers.AnyAsync()) return;

        var tenantInfo = await serviceProvider
            .GetRequiredService<ITenantDatabaseResolver>()
            .GetByDisplayName("default");

        if (tenantInfo is null || tenantInfo.BranchIds.Count < 2) return;

        var fromBranchId = tenantInfo.BranchIds[0];
        var toBranchId = tenantInfo.BranchIds[1];

        var productNames = InventorySeedData.Products.Select(p => p.Name).ToList();

        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .Include(pv => pv.Color)
            .Include(pv => pv.Size)
            .Where(pv => productNames.Contains(pv.Product.Name))
            .ToListAsync();

        var createTransfer = serviceProvider.GetRequiredService<CreateStockTransfer>();
        var resolveTransfer = serviceProvider.GetRequiredService<ResolveStockTransfer>();

        var transfers = new[]
        {
            ("Air Max 90", "Negro", "42", 3),
            ("Revolution 7", "Plomo", "40", 5),
        };

        foreach (var (productName, colorName, sizeName, quantity) in transfers)
        {
            var variant = variants.FirstOrDefault(pv =>
                pv.Product.Name == productName && pv.Color.Name == colorName && pv.Size.Name == sizeName);

            if (variant is null) continue;

            var createResult = await createTransfer.Execute(
                new ActorContext(tenantInfo.TenantId, tenantInfo.OwnerUserId, "System", fromBranchId, [fromBranchId]),
                new CreateStockTransferDto
                {
                    ToBranchId = toBranchId,
                    Notes = "Traspaso inicial entre sucursales",
                    Items = [new StockTransferItemDto { ProductVariantId = variant.Id, QuantityRequested = quantity }]
                });

            if (!createResult.IsSuccess)
                throw new InvalidOperationException(
                    $"Seeding transfer for {productName}/{colorName}/{sizeName} failed: {createResult.Error?.Code} - {createResult.Error?.Message}");

            var transfer = await context.StockTransfers
                .Where(t => t.FromBranchId == fromBranchId && t.ToBranchId == toBranchId && t.Status == TransferStatus.Pending)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (transfer is null)
                throw new InvalidOperationException("Created transfer could not be resolved back from the database");

            var resolveResult = await resolveTransfer.Execute(
                new ActorContext(tenantInfo.TenantId, tenantInfo.OwnerUserId, "System", toBranchId, [toBranchId]),
                transfer.Id,
                new ResolveStockTransferDto { Complete = true, Notes = "Traspaso inicial entre sucursales" });

            if (!resolveResult.IsSuccess)
                throw new InvalidOperationException(
                    $"Seeding resolve for {productName}/{colorName}/{sizeName} failed: {resolveResult.Error?.Code} - {resolveResult.Error?.Message}");
        }
    }
}