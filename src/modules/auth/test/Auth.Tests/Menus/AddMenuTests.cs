using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using System;
using Auth.Data.Persistence;
using Auth.UseCases.Menus;
using Auth.Dtos.Modules;
using Auth.Data.Entities;

namespace Auth.Tests.Menus;

public class AddMenuTests
{
    // Método auxiliar para crear un DbContext en memoria fresco para cada prueba.
    private AuthDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        
        // Asumiendo que AuthDbContext tiene un constructor que acepta DbContextOptions<AuthDbContext>
        return new AuthDbContext(options);
    }

    // ----------------------------------------------------------------------
    // TEST 1: Verificar que se crea un nuevo menú correctamente.
    // ----------------------------------------------------------------------
    [Fact]
    public async Task Execute_ShouldCreateNewMenu_WhenMenuDoesNotExist()
    {
        // ARRANGE
        // Usamos un nombre de base de datos único para aislar esta prueba.
        var dbContext = CreateInMemoryDbContext(Guid.NewGuid().ToString()); 
        var addMenuUseCase = new AddMenu(dbContext);
        
        var newMenuDto = new CreateMenuDto
        {
            Name = "Dashboard",
            Route = "/dashboard",
            ModuleId = 1,
            ParentMenuId = 0,
            Icon = "home",
            Order = 1
        };

        // ACT
        var result = await addMenuUseCase.Execute(newMenuDto);

        // ASSERT
        // 1. Verifica que el resultado sea exitoso.
        Assert.True(result.IsSuccess);
        // 2. Verifica que el resultado devuelva un ID (un entero positivo).
        Assert.True(result.Value > 0);

        // 3. Verifica que la entidad haya sido realmente guardada en la base de datos.
        var createdMenu = await dbContext.Menus.FindAsync(result.Value);
        Assert.NotNull(createdMenu);
        Assert.Equal(newMenuDto.Name, createdMenu.Name);
        Assert.Equal(newMenuDto.ModuleId, createdMenu.ModuleId);
    }

    // ----------------------------------------------------------------------
    // TEST 2: Verificar que falle cuando ya existe un menú con el mismo nombre y módulo.
    // ----------------------------------------------------------------------
    [Fact]
    public async Task Execute_ShouldReturnDuplicateError_WhenMenuAlreadyExistsInModule()
    {
        // ARRANGE
        var dbName = Guid.NewGuid().ToString();
        var dbContext = CreateInMemoryDbContext(dbName);
        var addMenuUseCase = new AddMenu(dbContext);
        
        // Datos a usar en ambos casos (el que ya existe y el que se intenta crear)
        var sharedMenuData = new CreateMenuDto
        {
            Name = "Settings",
            ModuleId = 2, // Mismo módulo
            Route = "/settings",
            Order = 1
        };
        dbContext.Menus.Add(new Menu 
        { 
            Name = sharedMenuData.Name, 
            ModuleId = sharedMenuData.ModuleId, 
            Route = sharedMenuData.Route, 
            Order = sharedMenuData.Order 
        });
        await dbContext.SaveChangesAsync();
        var result = await addMenuUseCase.Execute(sharedMenuData);
        Assert.False(result.IsSuccess);
        Assert.Equal("DUPLICATE", result.Error!.Code);
        Assert.Contains("Ya existe un menú con ese nombre en el mismo modulo", result.Error.Message);
        
        var count = await dbContext.Menus.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Execute_ShouldCreateNewMenu_WhenNameIsSameButModuleIsDifferent()
    {
        var dbName = Guid.NewGuid().ToString();
        var dbContext = CreateInMemoryDbContext(dbName);
        var addMenuUseCase = new AddMenu(dbContext);

        var existingMenu = new Menu { Name = "Reports", ModuleId = 1, Route = "/reports/mod1", Order = 1 };
        dbContext.Menus.Add(existingMenu);
        await dbContext.SaveChangesAsync();
        
        var newMenuDto = new CreateMenuDto
        {
            Name = "Reports",
            ModuleId = 2, 
            Route = "/reports/mod2",
            Order = 1
        };
        var result = await addMenuUseCase.Execute(newMenuDto);
        Assert.True(result.IsSuccess);
        var count = await dbContext.Menus.CountAsync();
        Assert.Equal(2, count);
    }
}