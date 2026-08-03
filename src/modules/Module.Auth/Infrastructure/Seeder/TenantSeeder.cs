using Common.Contracts.authentication;
using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Seeder;

public class TenantSeeder(IAuthDbContext context, ITenantConnectionContext tenantConnectionContext) : IDataSeeder
{
    public int Order => 4;

    public async Task SeedAsync()
    {
        if (await context.Tenants.AnyAsync())
        {
            var existing = await context.Tenants
                .Include(t => t.TenantDataBase)
                .FirstAsync();
            tenantConnectionContext.TenantId = existing.Id;
            tenantConnectionContext.DatabaseName = existing.TenantDataBase.Name;
            tenantConnectionContext.Schema = existing.TenantDataBase.Schema;
            return;
        }

        var db = await context.TenantDatabases.FirstAsync(x => x.Name == "erp_db");
        var plan = await context.Plans.FirstAsync(p => p.Name == "Basic");

        var tenantId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        tenantConnectionContext.TenantId = tenantId;
        tenantConnectionContext.DatabaseName = db.Name;
        tenantConnectionContext.Schema = db.Schema;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("1234");

        var ownerUser = new User
        {
            Id = ownerUserId,
            Email = "admin@drivecore.com",
            Username = "admin",
            FirstName = "Admin",
            LastName = "Admin",
            PasswordHash = passwordHash,
            Status = UserStatus.Ready,
            IsActive = true,
            Type = UserType.Owner,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = ownerUserId,
            CreatedByName = "Admin Admin",
        };

        var tenant = Tenant.Create(tenantId, "default", db.Id, plan.Id, ownerUser, ownerUserId, "Admin Admin");
        context.Tenants.Add(tenant);

        var features = await context.Features
            .Select(f => new FeatureModuleInfo(f.Key, f.Module))
            .ToListAsync();

        var mainBranch = Branch.Create(Guid.NewGuid(), "Main Branch", "Av. Principal 123", "000000000", BranchType.PointOfSale, ownerUserId, "Admin Admin");
        mainBranch.AllowedFeatureKeys = BranchFeatureKeysResolver.Resolve(plan.AllowedFeatureKeys, mainBranch.Type, features);
        var secondaryBranch = Branch.Create(Guid.NewGuid(), "Secondary Branch", "Av. Secundaria 456", "000000001", BranchType.Warehouse, ownerUserId, "Admin Admin");
        secondaryBranch.AllowedFeatureKeys = BranchFeatureKeysResolver.Resolve(plan.AllowedFeatureKeys, secondaryBranch.Type, features);
        context.Branches.Add(mainBranch);
        context.Branches.Add(secondaryBranch);

        foreach (var roleTemplate in plan.DefaultRolesTemplate)
        {
            var role = Role.CreateFromTemplate(roleTemplate, ownerUserId, "Admin Admin");
            context.Roles.Add(role);
        }

        await context.SaveChangesAsync();
    }
}
