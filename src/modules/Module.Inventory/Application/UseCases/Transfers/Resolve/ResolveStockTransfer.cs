using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Application.UseCases.Transfers.Resolve;

public class ResolveStockTransfer(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid transferId, ResolveStockTransferDto dto)
    {
        var toBranchId = currentUser.BranchIds[0];

        var transfer = await context.StockTransfers
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == transferId);

        if (transfer == null)
            return ResolveStockTransferErrors.TransferNotFound;

        if (transfer.ToBranchId != toBranchId)
            return ResolveStockTransferErrors.Forbidden;

        if (transfer.Status != TransferStatus.Pending)
            return ResolveStockTransferErrors.AlreadyResolved;

        if (!dto.Complete)
        {
            transfer.Reject(currentUser.UserId, dto.Notes);
            await context.SaveChangesAsync();
            return true;
        }

        // Validar stock de nuevo por si cambió desde que se creó
        var variantIds = transfer.Items.Select(x => x.ProductVariantId).ToList();
        var inventories = await context.BranchInventories
            .Where(bi => bi.BranchId == transfer.FromBranchId && variantIds.Contains(bi.ProductVariantId))
            .ToListAsync();

        var insufficientStock = transfer.Items
            .Where(item => inventories.First(inv => inv.ProductVariantId == item.ProductVariantId).Stock < item.QuantityRequested)
            .ToList();

        if (insufficientStock.Count != 0)
            return ResolveStockTransferErrors.InsufficientStock;

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            transfer.Accept(currentUser.UserId, dto.Notes);
            var productVariants = await context.ProductVariants
                .Include(pv => pv.BranchInventories)
                .Where(pv => variantIds.Contains(pv.Id))
                .ToListAsync();
            foreach (var item in transfer.Items)
            {
                var productVariant = productVariants.First(pv => pv.Id == item.ProductVariantId);

                productVariant.AddQuantity(-item.QuantityRequested, transfer.FromBranchId);
                productVariant.AddQuantity(item.QuantityRequested, transfer.ToBranchId);

                // StockMovements
                var (movOut, movIn) = StockMovement.CreateTransfer(
                    transfer.FromBranchId,
                    transfer.ToBranchId,
                    item.ProductVariantId,
                    currentUser.UserId,
                    item.QuantityRequested,
                    transfer.Id

                );
                transfer.StockMovements.Add(movIn);
                transfer.StockMovements.Add(movOut);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return true;
    }


}