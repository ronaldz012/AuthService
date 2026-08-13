using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;

namespace Module.Inventory.Application.UseCases.Products.UpdateStatus;

public class UpdateProductStatus(IInvDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<bool>> Execute(Guid id, UpdateProductStatusDto dto)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return UpdateProductStatusErrors.ProductNotFound;

        product.IsActive = dto.IsActive;
        product.UpdatedBy = currentUser.UserId;
        product.UpdatedByName = currentUser.FullName;
        product.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return product.IsActive;
    }
}