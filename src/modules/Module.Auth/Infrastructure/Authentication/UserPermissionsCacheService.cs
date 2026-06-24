using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Infrastructure.Authentication;

public class UserPermissionsCacheService(
    IMemoryCache cache,
    IAuthDbContext context) : IUserPermissionsCacheService
{
    private static readonly MemoryCacheEntryOptions Opts =
        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    private static string Key(Guid userId) => $"user_branches:{userId}";

    public async Task<List<PermissionsDto>> GetAsync(Guid userId, bool isAdmin)
    {
        // 1. Retornar del caché si existe
        if (cache.TryGetValue(Key(userId), out List<PermissionsDto>? cached) && cached is not null)
            return cached;

        // 2. Consulta Centralizada
        var user = await context.Users
            .AsSplitQuery()
            .Include(u => u.UserBranchRoles)
                .ThenInclude(ubr => ubr.Branch)
            .Include(u => u.UserBranchRoles)
                .ThenInclude(ubr => ubr.Role)
                    .ThenInclude(r => r.RoleFeaturePermissions)
                        .ThenInclude(rfp => rfp.Feature)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.UserBranchRoles.Any()) return [];

        var userBranchRoles = user.UserBranchRoles;
        List<PermissionsDto> branches;

        if (isAdmin)
        {
            var allFeatures = await context.Features.ToListAsync();
            
            var userBranches = userBranchRoles
                .Select(ubr => ubr.Branch)
                .DistinctBy(b => b.Id)
                .ToList();

            branches = userBranches.Select(branch => new PermissionsDto
            {
                BranchId   = branch.Id,
                BranchName = branch.Name,
                Roles      = [],
                Features   = allFeatures.Select(f => new FeaturePermissionsDeductedDto
                {
                    Id          = f.Id,
                    ModuleName  = f.Name, 
                    Permissions = ["*"] // Acceso total para Admin
                }).ToList()
            }).ToList();
        }
        else
        {
            // 3. Extracción directa y limpia (1 Rol por Branch garantizado)
            branches = userBranchRoles.Select(ubr => new PermissionsDto
            {
                BranchId   = ubr.Branch.Id,
                BranchName = ubr.Branch.Name,
                
                // Solo hay un rol por registro, mapeamos directo a la lista
                Roles = [new RoleDto 
                { 
                    Id   = ubr.Role.Id, 
                    Name = ubr.Role.Name 
                }],
                
                // Mapeo directo uno a uno de las características asignadas a ese rol único
                Features = ubr.Role.RoleFeaturePermissions.Select(rfp => new FeaturePermissionsDeductedDto
                {
                    Id          = rfp.Feature.Id,
                    ModuleName  = rfp.Feature.Name,
                    Permissions = rfp.Permissions // Pasamos tu nuevo array de strings directamente
                }).ToList()
            }).ToList();
        }

        // 4. Guardar en caché y retornar
        cache.Set(Key(userId), branches, Opts);
        return branches;
    }

    public void Invalidate(Guid userId) => cache.Remove(Key(userId));

    public void Set(Guid userId, List<PermissionsDto> branches) => cache.Set(Key(userId), branches, Opts);
}