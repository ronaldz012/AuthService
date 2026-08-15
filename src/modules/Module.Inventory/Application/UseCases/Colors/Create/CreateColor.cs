using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Colors.Create;

public class CreateColor(IInvDbContext context)
{
    public async Task<Result<ColorDto>> Execute(ActorContext ctx, string colorName)
    {
        // Buscamos si existe conflicto con nombre (case-insensitive, incluyendo eliminados si usas filtros globales)
        var existing = await context.Colors.AnyAsync(x => x.Name.ToLower() == colorName.ToLower());


        if (existing)
        {
    
            return CreateColorErrors.ColorAlreadyExists;
        }

        var newColor = new Color
        {
            Name = colorName,
            CreatedBy = ctx.UserId,
            CreatedByName = ctx.FullName
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