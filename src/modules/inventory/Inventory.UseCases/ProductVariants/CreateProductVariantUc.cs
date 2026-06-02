using Common.Result;
using Inventory.Contracts.Dtos.ProductVariants;
using Inventory.Data;
using Inventory.Data.Entities.Products;
using Microsoft.EntityFrameworkCore;


namespace Inventory.UseCases.ProductVariants;


public class CreateProductVariantUc(InvDbContext context)
{
    public async Task<Result<List<ProductVariantCreatedDto>>> Execute(Guid productId, List<CreateProductVariantDto> dto)
    {
        // 1. Validar que la lista no venga vacía
        if (dto == null || dto.Count == 0)
            return new Error("BAD REQUEST", "The variant list cannot be empty.");

        // 2. Validar duplicados dentro del mismo DTO (petición)
        var hasDuplicatesInDto = dto
            .GroupBy(x => new { x.ColorId, Size = x.Size.Trim().ToLower() })
            .Any(g => g.Count() > 1);

        if (hasDuplicatesInDto)
            return new Error("BAD REQUEST", "There are duplicate variants (same size and color) in your request.");

        // 3. Validar si el producto existe
        var product = await context.Products
            .Select(x => new { x.Id, x.InternalCode })
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
            return new Error("NOT FOUND", "Product not found");

        // 4. Validar y mapear colores a un Diccionario
        var colorIdsInDto = dto.Select(x => x.ColorId).Distinct().ToList();
        
        var colorsDictionary = await context.Colors
            .Where(c => colorIdsInDto.Contains(c.Id))
            .Select(x => new { x.Id, x.Code, x.Name })
            .ToDictionaryAsync(x => x.Id, x => x);

        // Validar si falta algún color en la base de datos
        if (colorsDictionary.Count != colorIdsInDto.Count)
        {
            var missingIds = colorIdsInDto.Where(id => !colorsDictionary.ContainsKey(id)).ToList();
            return new Error("NOT FOUND", $"The following Color IDs do not exist: {string.Join(", ", missingIds)}");
        }

        // 5. Validar contra las variantes ya existentes en la Base de Datos
        var dtoCombinations = dto.Select(d => new { d.ColorId, Size = d.Size.Trim().ToLower() }).ToList();
        var colorIdsToCheck = dtoCombinations.Select(d => d.ColorId).Distinct().ToList();
        var sizesToCheck = dtoCombinations.Select(d => d.Size).Distinct().ToList();

        // Traemos solo las variantes del producto que coincidan con ALGUNO de los colores o tallas del DTO
        var existingVariants = await context.ProductVariants
            .Where(pv => pv.ProductId == productId 
                    && colorIdsToCheck.Contains(pv.ColorId) 
                    && sizesToCheck.Contains(pv.Size.ToLower()))
            .Select(pv => new { pv.ColorId, Size = pv.Size.ToLower() })
            .ToListAsync(); // <-- Evaluamos en el servidor y traemos solo los candidatos potenciales

        // Ahora hacemos el cruce exacto en memoria (Client Evaluation), que es ultra rápido
        var existingVariantExists = existingVariants
            .Any(ev => dtoCombinations.Any(d => d.ColorId == ev.ColorId && d.Size == ev.Size));

        if (existingVariantExists)
            return new Error("DUPLICATED", "One or more variants with the same size and color already exist for this product.");

        // 6. Creación de las nuevas variantes
        var variants = dto.Select(x => 
        {
            // Obtenemos el color correspondiente desde el diccionario de forma segura
            var color = colorsDictionary[x.ColorId]; 
            
            return new ProductVariant
            {
                ProductId = productId,
                Size = x.Size.Trim(), // Guardamos limpio
                ColorId = x.ColorId,
                Description = x.Description,
                Price = x.Price,
                // Pasamos el color correcto y no el primero de la lista
                Sku = ProductVariant.GenerateSku(product.InternalCode, color.Code, x.Size.Trim()) 
            };
        }).ToList();

        // Nota: Si usas EF Core, para listas se recomienda 'AddRange' en lugar de 'Add'
        context.ProductVariants.AddRange(variants); 
        await context.SaveChangesAsync();

        // 7. Retornar DTO de respuesta
        return variants.Select(v => new ProductVariantCreatedDto
        {
            ProductVariantId = v.Id,
            Sku = v.Sku,
            Size = v.Size,
            ColorName = colorsDictionary[v.ColorId].Name
        }).ToList();
    }
}