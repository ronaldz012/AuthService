using Auth.Contracts.Interfaces;
using Inventory.Contracts.Dtos.Receptions;
using Inventory.Data.Entities.Inventory;
using Inventory.Data.Entities.Receptions;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Receptions;

public class CreateReceptionUc(
    InvDbContext context,
    ICurrentUser currentUser)
{
    public async Task<Result<StockReceptionResultDto>> Execute(CreateStockReceptionDto dto)
    {
        var userId = currentUser.UserId;
        var branchId = currentUser.BranchIds[0];

        // -- 1. Validar que todas las variantes existen -----------------------
        var variantIds = dto.Items.Select(x => x.ProductVariantId).ToList();

        var variants = await context.ProductVariants
            .Include(x => x.BranchInventories)
            .Where(x => variantIds.Contains(x.Id))
            .ToListAsync();

        var missingIds = variantIds.Except(variants.Select(x => x.Id)).ToList();
        if (missingIds.Count != 0)
            return new Error("NOT_FOUND",
                $"VariantIds no encontrados: {string.Join(", ", missingIds)}");

        // -- 2. Construir recepción -------------------------------------------
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var newReception = new StockReception
            {
                Id = new Guid(),
                BranchId = branchId,
                Notes = dto.Notes,
                ReceivedAt = DateTime.UtcNow
            };
            var receptionId = newReception.Id;

            var stockMovements = new List<StockMovement>();
            var variantMap = variants.ToDictionary(v => v.Id);

            foreach (var item in dto.Items)
            {
                var variant = variantMap[item.ProductVariantId];

                newReception.Items.Add(new StockReceptionItem
                {
                    ProductVariantId = variant.Id,
                    QuantityReceived = item.QuantityReceived,
                    UnitCost = item.UnitCost
                });

                variant.AddQuantity(item.QuantityReceived, branchId);

                stockMovements.Add(StockMovement.CreateReception(
                    branchId, variant.Id, userId, item.QuantityReceived, receptionId));
            }

            context.StockReceptions.Add(newReception);
            context.StockMovements.AddRange(stockMovements);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            // -- 3. Retornar resultado ----------------------------------------
            var result = await context.StockReceptions
                .Where(r => r.Id == newReception.Id)
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
                throw new Exception($"La recepción {newReception.Id} se guardó pero no pudo ser consultada.");

            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}