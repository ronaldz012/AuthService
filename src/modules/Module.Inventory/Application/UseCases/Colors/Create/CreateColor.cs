using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Colors.Create;

public class CreateColor(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<ColorDto>> Execute(string colorName)
    {
        // Buscamos si existe conflicto con nombre o código (incluyendo eliminados si usas filtros globales)
        var existing = await context.Colors.AnyAsync(x => x.Name == colorName );


        if (existing)
        {
    
            return CreateColorErrors.ColorAlreadyExists;
        }

        var newColor = new Color
        {
            Name = colorName,
            CreatedBy = currentUser.UserId,
            CreatedByName = currentUser.FullName
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