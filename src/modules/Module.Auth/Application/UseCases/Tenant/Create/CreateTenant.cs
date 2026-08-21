using Common.Contracts.authentication;
using Common.Contracts.inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Module.Auth.Application.UseCases.Tenant.Create;

using global::Common.Utilities;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;
public class CreateTenant(
    IAuthDbContext context,
    ITenantConnectionContext tenantConnectionContext,
    IAuth0ProvisioningService auth0Provisioning,
    IDefaultCatalogProvisioner catalogProvisioner,
    IOptions<ProjectInfo> projectInfo,
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

        var invitationResult = await auth0Provisioning.CreateInvitationUserAsync(request.OwnerEmail);
        if (!invitationResult.IsSuccess)
            return invitationResult.Error;
        var auth0Id = invitationResult.Value;

        var resultUrl = $"https://{projectInfo.Value.AppBranding.FrontendDomain}/login";
        var ticketResult = await auth0Provisioning.CreatePasswordChangeTicketAsync(auth0Id, resultUrl);
        if (!ticketResult.IsSuccess)
            return ticketResult.Error;
        var ticket = ticketResult.Value;
        var ticketExpiresAt = DateTime.UtcNow.AddSeconds(432000);

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var tenantId = Guid.NewGuid();
            var ownerUserId = Guid.NewGuid();
            var mainBranchId = Guid.NewGuid();

            tenantConnectionContext.TenantId = tenantId;

            var ownerUser = User.CreateOwner(ownerUserId, request.OwnerEmail, request.OwnerUserName, ownerUserId, request.OwnerEmail);
            ownerUser.ExternalAuthId = auth0Id;
            ownerUser.AuthProvider = AuthProvider.Auth0;
            ownerUser.PasswordChangeTicket = ticket;
            ownerUser.PasswordChangeTicketExpiresAt = ticketExpiresAt;
            var tenant = Tenant.Create(tenantId, request.DisplayName, db.Id, plan.Id, ownerUser, ownerUserId, request.OwnerEmail);
            context.Tenants.Add(tenant);

            var features = await context.Features
                .Select(f => new FeatureModuleInfo(f.Key, f.Module))
                .ToListAsync();
            var branchFeatureKeys = BranchFeatureKeysResolver.Resolve(plan.AllowedFeatureKeys, BranchType.PointOfSale, features);

            var mainBranch = Branch.Create(mainBranchId, request.BranchName, request.BranchPlace, request.BranchPhoneNumber, BranchType.PointOfSale, ownerUserId, request.OwnerEmail);
            mainBranch.AllowedFeatureKeys = branchFeatureKeys;
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

            var response = new CreateTenantResponse(ticket, ticket, request.DisplayName);
            
            return response;

      
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
