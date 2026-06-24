using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Colors.CreateColor;

public class CreateColor(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<ColorDto>> Execute(string colorName)
    {
        // Buscamos si existe conflicto con nombre o código (incluyendo eliminados si usas filtros globales)
        var existing = await context.Colors.AnyAsync(x => x.Name == colorName );


        if (existing)
        {
    
            // Retornamos DUPLICATE para que tu mapper devuelva un Status 409
            return new Error("DUPLICATE", $"El {colorName} ya está en uso.");
        }

        var newColor = new Color
        {
            Name = colorName,
            CreatedById = currentUser.UserId
        };

        context.Add(newColor);
        await context.SaveChangesAsync();

        return new ColorDto
        {
            Id = newColor.Id,
            Name = newColor.Name,
        };
    }
}