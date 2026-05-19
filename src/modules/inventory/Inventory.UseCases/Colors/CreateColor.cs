using Auth.Contracts.Interfaces;
using Common.Result;
using Inventory.Contracts.Dtos;
using Inventory.Data;
using Inventory.Data.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UseCases.Colors;

public class CreateColor(InvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<ColorDto>> Execute(CreateColorDto color)
    {
        // Buscamos si existe conflicto con nombre o código (incluyendo eliminados si usas filtros globales)
        var existing = await context.Colors
            .IgnoreQueryFilters() // Importante para no repetir códigos de colores "borrados"
            .Where(x => x.Name == color.Name || x.Code == color.Code)
            .Select(x => new { x.Name, x.Code })
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            // Determinamos cuál es el culpable para dar un mensaje específico
            string field = existing.Name == color.Name ? "nombre" : "código";
        
            // Retornamos DUPLICATE para que tu mapper devuelva un Status 409
            return new Error("DUPLICATE", $"El {field} ya está en uso.");
        }

        var newColor = new Color
        {
            Name = color.Name,
            Code = color.Code.ToUpper(),
            CreatedById = currentUser.UserId
        };

        context.Add(newColor);
        await context.SaveChangesAsync();

        return new ColorDto
        {
            Id = newColor.Id,
            Name = newColor.Name,
            Code = newColor.Code,
        };
    }
}