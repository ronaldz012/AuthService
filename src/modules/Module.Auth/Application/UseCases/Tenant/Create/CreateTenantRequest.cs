namespace Module.Auth.Application.UseCases.Tenant.Create;

public record CreateTenantRequest(
    string DisplayName,
    string OwnerEmail,
    Guid DatabaseId,
    string OwnerUserName,
    string BranchName,
    string BranchPlace,
    string BranchPhoneNumber, Guid PlanId);
    
