using System;
using Auth.Data.Persistence;
using Shared.Result;

namespace Auth.UseCases.Menus;

public class DeleteMenu(AuthDbContext dbContext)
{
    public async Task<Result<bool>> Execute(int id)
    {
        var menu = await dbContext.Menus.FindAsync(id);
        if (menu == null)
            return new Error("NOT_FOUND", "menu not found");
        dbContext.Menus.Remove(menu);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
