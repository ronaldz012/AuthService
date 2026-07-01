using Common.Contracts.branches;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.CreateUser;

public class CreateUser(IAuthDbContext context, IBranchService branchService)
{
    // Recibir usuario con datos completos, recibir roleIds, recibir branchIds:
    public async Task<Result<bool>> Execute(CreateUserRequest dto)
    {
        var validation = await context.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username);
        if (validation) return new Error("INVALID_OPERATION", "email or username taken");
    
        var branchIds = dto.BranchRoles.Select(br => br.BranchId).Distinct().ToList();
        var roleIds = dto.BranchRoles.Select(br => br.RoleId).Distinct().ToList();
    
        var branchesResult = await branchService.GetBranchesByIds(branchIds);
        var rolesResult = await ValidateRoles(roleIds);
    
        if (!branchesResult.IsSuccess) return new Error("NOT_FOUND", branchesResult.Error?.Message ?? "");
        if (!rolesResult.IsSuccess) return new Error("NOT_FOUND", rolesResult.Error?.Message ?? "");
    
        // 1. Generar hash de una sola vía usando BCrypt (maneja el salt internamente)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
    
        // 2. Crear la entidad limpia
        var newUser = new User
        {
            Email = dto.Email,
            Username = dto.Username,
            PasswordHash = passwordHash, // Guardamos el string generado
            Status = UserStatus.Active,
            UserBranchRoles = dto.BranchRoles.Select(br => new UserBranchRole
            {
                BranchId = br.BranchId,
                RoleId = br.RoleId,
            }).ToList()
        };
    
        context.Add(newUser);
        await context.SaveChangesAsync();
        
        return true;
    }
    
    private async Task<Result<bool>> ValidateRoles(List<Guid> roleIds)
    {
        // Optimizamos trayendo solo los Ids de la base de datos
        var foundRolesIds = await context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync();
        
        var missingRoleIds = roleIds.Except(foundRolesIds).ToList();
        
        if (missingRoleIds.Any())
        {
            // Usamos string.Join para que imprima los IDs correctamente en el texto
            return new Error("NOT_FOUND", $"roles not found, missing: {string.Join(", ", missingRoleIds)}");
        }
        
        return true;
    }
}