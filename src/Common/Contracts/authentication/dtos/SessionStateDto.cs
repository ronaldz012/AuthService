namespace Common.Contracts.authentication.dtos;

public record SessionStateDto(
    UserDetailResponse User,
    List<PermissionsByModuleDto> Branches,
    TenantPlanUsageDto TenantPlan
);
