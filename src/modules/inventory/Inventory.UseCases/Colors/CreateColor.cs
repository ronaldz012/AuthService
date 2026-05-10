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
        var unique = await context.Colors.AnyAsync(x => x.Name == color.Name || x.Code == color.Code);
        if(unique) return new Error("CONFLICT", "this color or code already exists");
        var newColor = new Color
        {
            Name = color.Name,
            Code = color.Code,
            CreatedById = currentUser.UserId
        };
        context.Add(newColor);
        await context.SaveChangesAsync();
        return new ColorDto()
        {
            Id = newColor.Id,
            Name = newColor.Name,
            Code = newColor.Code,
        };
    }
}