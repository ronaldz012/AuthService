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
                OwnerUser = new User
                {
                    Id = ownerUserId,
                    TenantId = tenantId,
                    Email = request.OwnerEmail,
                    Username = request.OwnerUserName,
                    PasswordHash = string.Empty,
                    Status = UserStatus.PendingVerification, 
                    CreatedAt = DateTime.UtcNow,
                    Type = UserType.Owner,
                }
            };
            context.Tenants.Add(tenant);
            
            var mainBranch = new Branch
            {
                Id = mainBranchId,
                TenantId = tenantId,
                Place = request.BranchPlace,
                PhoneNumber =  request.BranchPhoneNumber,
                Name = request.BranchName,
                CreatedAt = DateTime.UtcNow
            };
            context.Branches.Add(mainBranch);
            
            
            foreach (var roleTemplate in plan.DefaultRolesTemplate)
            {
                var newRole = new Role
                {
                    TenantId = tenantId,
                    Name = roleTemplate.Name,
                    Description = roleTemplate.Description,
                    CreatedAt = DateTime.UtcNow,
                    RoleFeaturePermissions = roleTemplate.Permissions.Select(permTemplate => new RoleFeaturePermission
                    {
                        FeatureKey = permTemplate.FeatureKey, 
                        Permissions = permTemplate.Actions, 
                        TenantId = tenantId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList()
                };
                context.Roles.Add(newRole);
            }
            
            
            var verificationCode = new EmailVerificationCode
            {
                TenantId = tenantId,
                UserId = ownerUserId,
                Email = request.OwnerEmail,
                Code = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddHours(48),
                Purpose = VerificationCodePurpose.AccountVerification,
                IsUsed = false
            };
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