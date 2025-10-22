using System;
using System.Net;
using Auth.Data.Persistence;
using Auth.Dtos.Modules;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace Auth.UseCases.Modules;

public class GetModule(AuthDbContext dbContext, IMapper mapper)
{
    public async Task<Result<ModuleDto?>> Execute(int id)
    {
        var module = await dbContext.Modules.Where(m => m.Id == id).Include(m => m.Menus).FirstOrDefaultAsync();
        if (module == null)
            return new Error("NOT_FOUND", "Module not found");

        return mapper.Map<ModuleDto?>(module);
    }
}
