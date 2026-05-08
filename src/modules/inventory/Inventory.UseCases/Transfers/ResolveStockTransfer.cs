using Auth.Contracts.Interfaces;
using Inventory.Contracts.Dtos.Transfers;
using Inventory.Data.Entities.Inventory;
using Inventory.Data.Entities.Transfers;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Common.Services;
using Inventory.Data;

namespace Inventory.UseCases.Transfers;

public class ResolveStockTransfer(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid transferId, ResolveStockTransferDto dto)
    {
        var toBranchId = currentUser.BranchIds[0];

        var transfer = await context.StockTransfers
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == transferId);

        if (transfer == null)
            return new Error("NOT_FOUND", "Transfer not found");

        // Validar que quien resuelve es la sucursal destino
        if (transfer.ToBranchId != toBranchId)
            return new Error("FORBIDDEN", "Only the destination branch can resolve this transfer");

        if (transfer.Status != TransferStatus.Pending)
            return new Error("INVALID_OPERATION", $"Transfer is already {transfer.Status}");

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
            return new Error("INVALID_OPERATION", "Insufficient stock in origin branch, transfer cannot be completed");

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
                    item.QuantityRequested
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