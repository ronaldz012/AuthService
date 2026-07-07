namespace Common.Contracts.authentication.dtos;

public record TenantPlanUsageDto(
    string PlanName,
    List<string> Features,
    int MaxUsers,
    int ActiveUsers,
    int MaxBranches,
    int ActiveBranches
);
