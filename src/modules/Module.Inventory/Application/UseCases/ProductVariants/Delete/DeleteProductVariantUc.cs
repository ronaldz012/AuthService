using Common.Contracts.authentication;
using Common.Utilities;
using Inventory.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UseCases.ProductVariants;

public class DeleteProductVariantUc(InvDbContext context, ICurrentUser currentUser)
{
   public async Task<Result<bool>> Execute(Guid id)
    {
        // 1. Traemos la entidad trackeada y el cálculo en un solo viaje a la base de datos
        var variantData = await context.ProductVariants
            .Where(x => x.Id == id)
            .Select(x => new 
            {
                Entity = x,
                HasMovements = x.StockMovements.Any() 
                // Nota: Si no tienes la propiedad de navegación mapeada, usa:
                // HasMovements = context.StockMovements.Any(sm => sm.ProductVariantId == id)
            })
            .FirstOrDefaultAsync();

        // 2. Validación de existencia
        if (variantData is null)
        {
            return new Error("NOT_FOUND", "Product variant not found.");
        }

        // 3. Validación de regla de negocio
        if (variantData.HasMovements)
        {
            return new Error(
                "CONFLICT", 
                "Cannot delete the product variant because it already has associated stock movements."
            );
        }

        // 4. Ejecutamos el método de tu modelo de dominio (Mantiene encapsulamiento)
        variantData.Entity.SoftDelete(currentUser.UserId);

        // 5. Persistimos los cambios en la base de datos
        var rowsAffected = await context.SaveChangesAsync();
        return rowsAffected > 0;
    }
}