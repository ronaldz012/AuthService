using System;
using Auth.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth.Tests.Menus;

public class DeleteMenuTest
{
    private AuthDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        // Asumiendo que AuthDbContext tiene un constructor que acepta DbContextOptions<AuthDbContext>
        return new AuthDbContext(options);
    }
    [Fact]
    public async Task DeleteMenu_ShouldReturnTrue_WhenMenuExists()
    {
        var dbContext = CreateInMemoryDbContext(Guid.NewGuid().ToString());

        var menuToDelete = new Auth.Data.Entities.Menu
        {
            Id = 1,
            Name = "Menu to Delete",
            ModuleId = 1
        };
        dbContext.Menus.Add(menuToDelete);
        await dbContext.SaveChangesAsync();

        var deleteMenu = new Auth.UseCases.Menus.DeleteMenu(dbContext);

        var result = await deleteMenu.Execute(menuToDelete.Id);

        Assert.True(result.IsSuccess);

        var deletedMenu = await dbContext.Menus.FindAsync(menuToDelete.Id);
        Assert.Null(deletedMenu);
    }

}
