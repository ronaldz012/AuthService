using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Application.UseCases.Transfers.Resolve;

public class ResolveStockTransfer(IInvDbContext context)
{
    public async Task<Result<bool>> Execute(ActorContext ctx, Guid transferId, ResolveStockTransferDto dto)
    {
        var toBranchId = ctx.BranchId;
        var userId = ctx.UserId;
        var userName = ctx.FullName;

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
            transfer.Reject(userId, userName, dto.Notes);
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
            transfer.Accept(userId, userName, dto.Notes);
            var productVariants = await context.ProductVariants
                .Include(pv => pv.BranchInventories)
                .Where(pv => variantIds.Contains(pv.Id))
                .ToListAsync();
            foreach (var item in transfer.Items)
            {
                var productVariant = productVariants.First(pv => pv.Id == item.ProductVariantId);

                var fromBefore = productVariant.GetStockByBranch(transfer.FromBranchId);
                var toBefore = productVariant.GetStockByBranch(transfer.ToBranchId);

                productVariant.AddQuantity(-item.QuantityRequested, transfer.FromBranchId, userId, userName);
                productVariant.AddQuantity(item.QuantityRequested, transfer.ToBranchId, userId, userName);

                var fromAfter = productVariant.GetStockByBranch(transfer.FromBranchId);
                var toAfter = productVariant.GetStockByBranch(transfer.ToBranchId);

                // StockMovements
                var (movOut, movIn) = StockMovement.CreateTransfer(
                    transfer.FromBranchId,
                    transfer.ToBranchId,
                    item.ProductVariantId,
                    userId,
                    userName,
                    item.QuantityRequested,
                    transfer.Id,
                    productVariant.AverageCost,
                    fromBefore, fromAfter, toBefore, toAfter
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