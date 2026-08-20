using Common.Contracts.authentication;
using Common.Contracts.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Module.Auth.Application.UseCases.Tenant.Create;

using global::Common.Utilities;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;
public class CreateTenant(
    IAuthDbContext context,
    ITenantConnectionContext tenantConnectionContext,
    IDefaultCatalogProvisioner catalogProvisioner,
    ILogger<CreateTenant> logger)
{
    public async Task<Result<CreateTenantResponse>> ExecuteAsync(CreateTenantRequest request)
    {
        var db = await context.TenantDatabases.FirstOrDefaultAsync(x => x.Id == request.DatabaseId);
        if (db == null)
            return CreateTenantErrors.DatabaseNotFound;
        var displayNameExists = await context.Tenants.AnyAsync(x => x.DisplayName == request.DisplayName);
        if (displayNameExists)
            return CreateTenantErrors.TenantAlreadyExists;
        var plan = await context.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId);
        if (plan == null)
            return CreateTenantErrors.PlanNotFound;

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var tenantId = Guid.NewGuid();
            var ownerUserId = Guid.NewGuid();
            var mainBranchId = Guid.NewGuid();

            tenantConnectionContext.TenantId = tenantId;

            var ownerUser = User.CreateOwner(ownerUserId, request.OwnerEmail, request.OwnerUserName, ownerUserId, request.OwnerEmail);
            var tenant = Tenant.Create(tenantId, request.DisplayName, db.Id, plan.Id, ownerUser, ownerUserId, request.OwnerEmail);
            context.Tenants.Add(tenant);

            var mainBranch = Branch.Create(mainBranchId, request.BranchName, request.BranchPlace, request.BranchPhoneNumber, BranchType.Warehouse, ownerUserId, request.OwnerEmail);
            context.Branches.Add(mainBranch);

            foreach (var roleTemplate in plan.DefaultRolesTemplate)
            {
                var role = Role.CreateFromTemplate(roleTemplate, ownerUserId, request.OwnerEmail);
                context.Roles.Add(role);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            try
            {
                await catalogProvisioner.SeedAsync(
                    tenantId,
                    ownerUserId,
                    request.OwnerEmail,
                    plan.DefaultCatalogTemplate ?? Common.Contracts.inventory.DefaultCatalogTemplates.Basic);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding default catalog for tenant {TenantId}", tenantId);
            }

            var response = new CreateTenantResponse(string.Empty, string.Empty, request.DisplayName);
            
            return response;

      
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
