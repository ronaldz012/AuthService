using System;
using Auth.Data.Persistence;
using Auth.Dtos.Modules;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace Auth.UseCases.Menus;

public class AddMenu(AuthDbContext dbContext)
{
    public async Task<Result<int>> Execute(CreateMenuDto dto)
    {
        // Solo validaciones de NEGOCIO, no de formato
        var exists = await dbContext.Menus
            .AnyAsync(m => m.Name == dto.Name && m.ModuleId == dto.ModuleId);
        
        if (exists)
            return new Error("DUPLICATE", "Ya existe un menú con ese nombre");

        try
        {
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
        catch (DbUpdateException ex)
        {
            // Extraer el mensaje más específico de las excepciones internas
            var errorMessage = GetInnerMostMessage(ex);
            
            // Detectar tipos específicos de errores de BD
            if (errorMessage.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
                return new Error("INVALID_REFERENCE", $"Referencia inválida a otra entidad {errorMessage}");
            
            if (errorMessage.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
                return new Error("DUPLICATE", $"Ya existe un registro con esos datos {errorMessage}");
            
            // Error genérico de BD
            return new Error("DATABASE_ERROR", errorMessage);
        }
    }

    /// <summary>
    /// Obtiene el mensaje de la excepción más interna (donde está el error real).
    /// </summary>
    private static string GetInnerMostMessage(Exception ex)
    {
        var innerException = ex;
        
        // Navegar hasta la excepción más interna
        while (innerException.InnerException != null)
        {
            innerException = innerException.InnerException;
        }
        
        return innerException.Message;
    }
}