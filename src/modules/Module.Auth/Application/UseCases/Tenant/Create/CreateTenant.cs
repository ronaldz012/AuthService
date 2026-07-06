using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Module.Auth.Application.UseCases.Tenant.Create;

using global::Common.Utilities;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;
using Module.Auth.Infrastructure.Authentication;
public class CreateTenant(
    IAuthDbContext context,
    ITenantContext tenantContext,
    IEmailVerificationService emailVerificationService,
    IOptions<ProjectInfo> projectInfo)
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

            tenantContext.TenantId = tenantId;

            var ownerUser = User.CreateOwner(ownerUserId, request.OwnerEmail, request.OwnerUserName);
            var tenant = Tenant.Create(tenantId, request.DisplayName, db.Id, plan.Id, ownerUser);
            context.Tenants.Add(tenant);

            var mainBranch = Branch.Create(mainBranchId, request.BranchName, request.BranchPlace, request.BranchPhoneNumber);
            context.Branches.Add(mainBranch);

            foreach (var roleTemplate in plan.DefaultRolesTemplate)
            {
                var role = Role.CreateFromTemplate(roleTemplate);
                context.Roles.Add(role);
            }

            var verificationCode = EmailVerificationCode.CreateForAccountSetup(request.OwnerEmail);
            ownerUser.EmailVerificationCodes.Add(verificationCode);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            var frontendDomain = projectInfo.Value.AppBranding.FrontendDomain;
            var setupUrl = $"https://{request.DisplayName}.{frontendDomain}/auth/setup-password?code={verificationCode.Code}";
            var response = new CreateTenantResponse(verificationCode.Code, setupUrl, request.DisplayName);

            if (request.SendEmail)
            {
                try
                {
                    await emailVerificationService.SendTenantSetupEmailAsync(
                        request.OwnerEmail,
                        request.OwnerUserName,
                        setupUrl,
                        verificationCode.ExpiresAt);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
              
                }

            }
            
            return response;

      
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
