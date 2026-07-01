using Common.Contracts.authentication;
using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Seeder;

public class TenantSeeder(IAuthDbContext context, ITenantContext tenantContext) : IDataSeeder
{
    public int Order => 4;

    public async Task SeedAsync()
    {
        if (await context.Tenants.AnyAsync()) return;

        var db = await context.TenantDatabases.FirstAsync(x => x.Name == "erp_db");
        var plan = await context.Plans.FirstAsync(p => p.Name == "Basic");

        var tenantId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var mainBranchId = Guid.NewGuid();

        tenantContext.TenantId = tenantId;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("1234");

        var ownerUser = new User
        {
            Id = ownerUserId,
            Email = "admin@drivecore.com",
            Username = "admin",
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
            Type = UserType.Owner,
            CreatedAt = DateTime.UtcNow,
        };

        var tenant = Tenant.Create(tenantId, "default", db.Id, plan.Id, ownerUser);
        context.Tenants.Add(tenant);

        var mainBranch = Branch.Create(mainBranchId, "Main Branch", "Default location", "000000000");
        context.Branches.Add(mainBranch);

        foreach (var roleTemplate in plan.DefaultRolesTemplate)
        {
            var role = Role.CreateFromTemplate(roleTemplate);
            context.Roles.Add(role);
        }

        await context.SaveChangesAsync();
    }
}
