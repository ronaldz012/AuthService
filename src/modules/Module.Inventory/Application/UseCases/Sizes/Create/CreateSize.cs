using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Products;

namespace Module.Inventory.Application.UseCases.Sizes.Create;

public class CreateSize(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<SizeDto>> Execute(CreateSizeDto dto)
    {
        var name = dto.Name.Trim();

        var existing = await context.Sizes.AnyAsync(x => x.Name.ToLower() == name.ToLower());
        if (existing)
            return CreateSizeErrors.SizeAlreadyExists;

        var size = new Size
        {
            Name = name,
            SortOrder = dto.SortOrder,
            CreatedBy = currentUser.UserId,
            CreatedByName = currentUser.FullName
        };

        context.Sizes.Add(size);
        await context.SaveChangesAsync();

        return new SizeDto
        {
            Id = size.Id,
            Name = size.Name,
            SortOrder = size.SortOrder
        };
    }
}