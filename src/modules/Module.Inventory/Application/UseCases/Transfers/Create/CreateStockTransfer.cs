using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Transfers;

namespace Module.Inventory.Application.UseCases.Transfers.Create;

public class CreateStockTransfer(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(CreateStockTransferDto dto)
    {
        var fromBranchId = currentUser.BranchIds[0];

        // Validar que no es la misma sucursal
        if (fromBranchId == dto.ToBranchId)
            return new Error("INVALID_OPERATION", "Cannot transfer to the same branch");

        // Validar stock suficiente en origen
        var variantIds = dto.Items.Select(x => x.ProductVariantId).ToList();
        var inventories = await context.BranchInventories
            .Where(bi => bi.BranchId == fromBranchId && variantIds.Contains(bi.ProductVariantId))
            .ToListAsync();

        var missingVariants = variantIds.Except(inventories.Select(x => x.ProductVariantId)).ToList();
        if (missingVariants.Count != 0)
            return new Error("NOT_FOUND", $"Variants not found in branch: {string.Join(", ", missingVariants)}");

        var insufficientStock = dto.Items
            .Where(item => inventories.First(inv => inv.ProductVariantId == item.ProductVariantId).Stock < item.QuantityRequested)
            .ToList();
        if (insufficientStock.Count != 0)
            return new Error("INVALID_OPERATION", "Insufficient stock for some variants");

        // Crear transferencia
        var transfer = new StockTransfer
        {
            FromBranchId = fromBranchId,
            ToBranchId = dto.ToBranchId,
            RequestedByUserId = currentUser.UserId,
            Notes = dto.Notes
        };

        foreach (var item in dto.Items)
        {
            transfer.Items.Add(new StockTransferItem
            {
                ProductVariantId = item.ProductVariantId,
                QuantityRequested = item.QuantityRequested
            });
        }

        context.StockTransfers.Add(transfer);
        await context.SaveChangesAsync();
        return true;
        
    }
}