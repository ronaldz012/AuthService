namespace Common.Contracts.authentication.dtos;

public record SessionStateDto(
    UserDetailResponse User,
    List<PermissionsByBranchDto> Branches,
    TenantPlanUsageDto TenantPlan
);