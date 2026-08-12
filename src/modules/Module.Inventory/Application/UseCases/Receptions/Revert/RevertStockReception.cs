using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Products;
using Module.Inventory.Domain.Receptions;

namespace Module.Inventory.Application.UseCases.Receptions.Revert;

public class RevertStockReception(
    IInvDbContext context,
    ICurrentUser currentUser,
    ILogger<RevertStockReception> logger)
{
    private const int MaxReceptionAgeDays = 1;

    public async Task<Result<StockReceptionRevertCheckDto>> Check(Guid id)
    {
        var loadResult = await LoadWithVariantsAsync(id);
        if (!loadResult.IsSuccess)
            return loadResult.Error;

        var (reception, variants) = loadResult.Value;
        var blockReason = GetBlockReason(reception, variants);

        return new StockReceptionRevertCheckDto
        {
            ReceptionId = id,
            CanRevert = blockReason == RevertBlockReason.None,
            Reason = ReasonString(blockReason)
        };
    }

    public async Task<Result<bool>> Execute(Guid id)
    {
        var loadResult = await LoadWithVariantsAsync(id);
        if (!loadResult.IsSuccess)
            return loadResult.Error;

        var (reception, variants) = loadResult.Value;
        var blockReason = GetBlockReason(reception, variants);
        if (blockReason != RevertBlockReason.None)
            return ReasonError(blockReason);

        var userId = currentUser.UserId;
        var branchId = reception.BranchId;

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in reception.Items)
            {
                var variant = variants.First(v => v.Id == item.ProductVariantId);
                variant.RemoveQuantity(item.QuantityReceived, branchId);

                context.StockMovements.Add(StockMovement.CreateReceptionRevert(
                    branchId, variant.Id, userId, currentUser.FullName, item.QuantityReceived, reception.Id));
            }

            reception.Status = ReceptionStatus.Reverted;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error al revertir recepción {ReceptionId} para sucursal {BranchId}", id, branchId);
            return RevertStockReceptionErrors.RevertFailed;
        }
    }

    private async Task<Result<(StockReception Reception, List<ProductVariant> Variants)>> LoadWithVariantsAsync(Guid id)
    {
        var branchId = currentUser.BranchIds[0];

        var reception = await context.StockReceptions
            .Include(r => r.Items)
            .Where(r => r.Id == id && r.BranchId == branchId)
            .FirstOrDefaultAsync();

        if (reception is null)
            return RevertStockReceptionErrors.ReceptionNotFound;

        var variantIds = reception.Items.Select(i => i.ProductVariantId).Distinct().ToList();

        var variants = variantIds.Count == 0
            ? []
            : await context.ProductVariants
                .Include(pv => pv.BranchInventories)
                .Where(pv => variantIds.Contains(pv.Id))
                .ToListAsync();

        return (reception, variants);
    }

    private static RevertBlockReason GetBlockReason(StockReception reception, List<ProductVariant> variants)
    {
        if (reception.Status == ReceptionStatus.Reverted)
            return RevertBlockReason.AlreadyReverted;

        if (reception.ReceivedAt < DateTime.UtcNow.AddDays(-MaxReceptionAgeDays))
            return RevertBlockReason.Outdated;

        var variantMap = variants.ToDictionary(v => v.Id);
        foreach (var item in reception.Items)
        {
            var variant = variantMap[item.ProductVariantId];
            if (!variant.HasSufficientStock(item.QuantityReceived, reception.BranchId))
                return RevertBlockReason.NotEnoughStock;
        }

        return RevertBlockReason.None;
    }

    private static string ReasonString(RevertBlockReason reason) => reason switch
    {
        RevertBlockReason.AlreadyReverted => "ALREADY_REVERTED",
        RevertBlockReason.Outdated => "OUTDATED",
        RevertBlockReason.NotEnoughStock => "NOT_ENOUGH_STOCK",
        _ => string.Empty
    };

    private static Error ReasonError(RevertBlockReason reason) => reason switch
    {
        RevertBlockReason.AlreadyReverted => RevertStockReceptionErrors.AlreadyReverted,
        RevertBlockReason.Outdated => RevertStockReceptionErrors.Outdated,
        RevertBlockReason.NotEnoughStock => RevertStockReceptionErrors.NotEnoughStock,
        _ => RevertStockReceptionErrors.RevertFailed
    };

    private enum RevertBlockReason
    {
        None,
        AlreadyReverted,
        Outdated,
        NotEnoughStock
    }
}