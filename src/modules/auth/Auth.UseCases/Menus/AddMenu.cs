using System;
using Auth.Data.Persistence;
using Auth.Dtos.Modules;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Result;

namespace Auth.UseCases.Menus;

public class AddMenu(AuthDbContext dbContext)
{
    public async Task<Result<int>> Execute(CreateMenuDto dto)
    {
      
            var exists = await dbContext.Menus
                .AnyAsync(m => m.Name == dto.Name && m.ModuleId == dto.ModuleId);
            
            if (exists)
                return new Error("DUPLICATE", "Ya existe un menú con ese nombre");
            var menu = new Data.Entities.Menu
            {
                Name = dto.Name,
                Route = dto.Route,
                ParentMenuId = dto.ParentMenuId,
                Icon = dto.Icon,
                Order = dto.Order,
                ModuleId = dto.ModuleId
            };

            dbContext.Menus.Add(menu);
            await dbContext.SaveChangesAsync();
            
            return menu.Id;
  
        

    }
}