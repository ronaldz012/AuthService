using Common.Contracts.authentication;
using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Branches.CreateBranch;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Seeder;

public class TenantSeeder(
    IAuthDbContext context,
    ITenantConnectionContext tenantConnectionContext,
    CreateBranch createBranch) : IDataSeeder
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
        await context.SaveChangesAsync();

        var actor = new ActorContext(tenantId, ownerUserId, "Admin Admin", Guid.Empty, []);

        var mainBranchResult = await createBranch.Execute(actor, new CreateBranchRequest
        {
            Name = "Main Branch",
            Place = "Av. Principal 123",
            PhoneNumber = "000000000",
            Type = BranchType.PointOfSale
        });

        if (!mainBranchResult.IsSuccess)
            throw new InvalidOperationException(
                $"Seeding Main Branch failed: {mainBranchResult.Error?.Code} - {mainBranchResult.Error?.Message}");

        var secondaryBranchResult = await createBranch.Execute(actor, new CreateBranchRequest
        {
            Name = "Secondary Branch",
            Place = "Av. Secundaria 456",
            PhoneNumber = "000000001",
            Type = BranchType.Warehouse
        });

        if (!secondaryBranchResult.IsSuccess)
            throw new InvalidOperationException(
                $"Seeding Secondary Branch failed: {secondaryBranchResult.Error?.Code} - {secondaryBranchResult.Error?.Message}");

        foreach (var roleTemplate in plan.DefaultRolesTemplate)
        {
            var role = Role.CreateFromTemplate(roleTemplate, ownerUserId, "Admin Admin");
            context.Roles.Add(role);
        }

        await context.SaveChangesAsync();
    }
}
