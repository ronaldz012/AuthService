using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Receptions;

namespace Module.Inventory.Application.UseCases.Receptions.Create;

public class CreateReceptionUc(
    IInvDbContext context,
    ICurrentUser currentUser,
    ILogger<CreateReceptionUc> logger)
{
    public async Task<Result<StockReceptionResultDto>> Execute(CreateStockReceptionDto dto)
    {
        var userId = currentUser.UserId;
        var branchId = currentUser.BranchIds[0];

        var variantIds = dto.Items.Select(x => x.ProductVariantId).ToList();

        var variants = await context.ProductVariants
            .Include(x => x.BranchInventories)
            .Include(x => x.Product)
            .Where(x => variantIds.Contains(x.Id))
            .ToListAsync();

        var missingIds = variantIds.Except(variants.Select(x => x.Id)).ToList();
        if (missingIds.Count != 0)
            return CreateReceptionErrors.VariantsNotFound;

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var reception = StockReception.Create(branchId, dto.Notes);
            var stockMovements = new List<StockMovement>();
            var variantMap = variants.ToDictionary(v => v.Id);

            foreach (var item in dto.Items)
            {
                var variant = variantMap[item.ProductVariantId];

                reception.AddExistingVariant(variant.Id, item.QuantityReceived, item.UnitCost);
                variant.AddQuantity(item.QuantityReceived, branchId);

                stockMovements.Add(StockMovement.CreateReception(
                    branchId, variant.Id, userId, item.QuantityReceived, reception.Id));
            }

            context.StockReceptions.Add(reception);
            context.StockMovements.AddRange(stockMovements);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            var result = await context.StockReceptions
                .Where(r => r.Id == reception.Id)
                .Select(r => new StockReceptionResultDto
                {
                    Id = r.Id,
                    BranchId = r.BranchId,
                    ReceivedAt = r.ReceivedAt,
                    Notes = r.Notes,
                    Items = r.Items.Select(i => new StockReceptionItemResultDto
                    {
                        ProductVariantId = i.ProductVariantId,
                        ProductName = i.ProductVariant.Product.Name,
                        VariantDescription = i.ProductVariant.Description,
                        QuantityReceived = i.QuantityReceived,
                        UnitCost = i.UnitCost
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return CreateReceptionErrors.ReceptionQueryFailed;

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error al crear recepción para sucursal {BranchId}", branchId);
            return CreateReceptionErrors.CreationFailed;
        }
    }
}
