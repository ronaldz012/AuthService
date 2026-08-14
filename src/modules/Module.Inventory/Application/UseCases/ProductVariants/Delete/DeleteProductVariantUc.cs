using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.ProductVariants.Delete;

public class DeleteProductVariantUc(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<ProductVariantDeleteCheckDto>> Check(Guid id)
    {
        var data = await context.ProductVariants
            .Where(x => x.Id == id)
            .Select(x => new
            {
                HasMovements = x.StockMovements.Any(),
                HasTransferItems = x.TransferItems.Any()
            })
            .FirstOrDefaultAsync();

        if (data is null)
            return DeleteProductVariantErrors.VariantNotFound;

        return new ProductVariantDeleteCheckDto
        {
            VariantId = id,
            CanDelete = !data.HasMovements && !data.HasTransferItems,
            Reason = data.HasMovements
                ? "HAS_MOVEMENTS"
                : data.HasTransferItems
                    ? "HAS_TRANSFER"
                    : string.Empty
        };
    }

    public async Task<Result<bool>> Execute(Guid id)
    {
        // 1. Traemos la entidad trackeada y el cálculo en un solo viaje a la base de datos
        var variantData = await context.ProductVariants
            .Where(x => x.Id == id)
            .Select(x => new 
            {
                Entity = x,
                HasMovements = x.StockMovements.Any(),
                HasTransferItems = x.TransferItems.Any()
            })
            .FirstOrDefaultAsync();

        if (variantData is null)
            return DeleteProductVariantErrors.VariantNotFound;

        if (variantData.HasMovements)
            return DeleteProductVariantErrors.VariantHasMovements;

        if (variantData.HasTransferItems)
            return DeleteProductVariantErrors.VariantHasTransfers;

        // 4. Ejecutamos el método de tu modelo de dominio (Mantiene encapsulamiento)
        variantData.Entity.SoftDelete(currentUser.UserId, currentUser.FullName);

        // 5. Persistimos los cambios en la base de datos
        var rowsAffected = await context.SaveChangesAsync();
        return rowsAffected > 0;
    }
}