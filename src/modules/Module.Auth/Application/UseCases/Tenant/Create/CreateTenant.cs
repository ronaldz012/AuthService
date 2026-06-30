using Microsoft.EntityFrameworkCore;

namespace Module.Auth.Application.UseCases.Tenant.Create;

using global::Common.Utilities;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;
public class CreateTenant(IAuthDbContext context)
{
    public async Task<Result<string>> ExecuteAsync(CreateTenantRequest request)
    {
        var db = await context.TenantDatabases.FirstOrDefaultAsync(x => x.Id == request.DatabaseId);
        if (db == null)
            return new Error("NOT_FOUND", $"Database {request.DatabaseId} not found");
        var displayNameExists = await context.Tenants.AnyAsync(x => x.DisplayName == request.DisplayName);
        if (displayNameExists)
            return new Error("VALIDATION_ERROR", $"Tenant {request.DisplayName} already exists");
        var plan = await context.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId);
        if (plan == null)
            return new Error("NOT_FOUND", "The specified subscription plan does not exist");

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var tenantId = Guid.NewGuid();
            var ownerUserId = Guid.NewGuid();
            var mainBranchId = Guid.NewGuid();

            var tenant = new Tenant
            {
                Id = tenantId,
                DisplayName = request.DisplayName,
                IsActive = true,
                DataBaseId = db.Id,
                PlanId = plan.Id,
                OwnerId = ownerUserId,
                OwnerUser = User.CreateOwner(ownerUserId, tenantId, request.OwnerEmail, request.OwnerUserName),
                CreatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenant);

            var mainBranch = Branch.Create(mainBranchId, tenantId, request.BranchName, request.BranchPlace, request.BranchPhoneNumber);
            context.Branches.Add(mainBranch);

            foreach (var roleTemplate in plan.DefaultRolesTemplate)
            {
                var role = Role.CreateFromTemplate(tenantId, roleTemplate);
                context.Roles.Add(role);
            }

            var verificationCode = EmailVerificationCode.CreateForAccountSetup(tenantId, ownerUserId, request.OwnerEmail);
            context.EmailVerificationCodes.Add(verificationCode);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return verificationCode.Code;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
