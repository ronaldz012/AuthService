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
            return CreateStockTransferErrors.SameBranchTransfer;

        // Validar stock suficiente en origen
        var variantIds = dto.Items.Select(x => x.ProductVariantId).ToList();
        var inventories = await context.BranchInventories
            .Where(bi => bi.BranchId == fromBranchId && variantIds.Contains(bi.ProductVariantId))
            .ToListAsync();

        var missingVariants = variantIds.Except(inventories.Select(x => x.ProductVariantId)).ToList();
        if (missingVariants.Count != 0)
            return CreateStockTransferErrors.VariantsNotFoundInBranch;

        var inactiveVariants = await context.ProductVariants
            .Where(v => variantIds.Contains(v.Id) && !v.Product.IsActive)
            .AnyAsync();
        if (inactiveVariants)
            return CreateStockTransferErrors.ProductInactive;

        var insufficientStock = dto.Items
            .Where(item => inventories.First(inv => inv.ProductVariantId == item.ProductVariantId).Stock < item.QuantityRequested)
            .ToList();
        if (insufficientStock.Count != 0)
            return CreateStockTransferErrors.InsufficientStock;

        // Crear transferencia
        var transfer = new StockTransfer
        {
            FromBranchId = fromBranchId,
            ToBranchId = dto.ToBranchId,
            RequestedByUserId = currentUser.UserId,
            Notes = dto.Notes,
            CreatedBy = currentUser.UserId,
            CreatedByName = currentUser.FullName
        };

        foreach (var item in dto.Items)
        {
            transfer.Items.Add(new StockTransferItem
            {
                ProductVariantId = item.ProductVariantId,
                QuantityRequested = item.QuantityRequested,
                CreatedBy = currentUser.UserId,
                CreatedByName = currentUser.FullName
            });
        }

        context.StockTransfers.Add(transfer);
        await context.SaveChangesAsync();
        return true;
        
    }
}