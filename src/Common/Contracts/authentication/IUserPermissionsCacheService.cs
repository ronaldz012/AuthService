using Common.Contracts.authentication.dtos;

namespace Common.Contracts.authentication;

public interface IUserPermissionsCacheService
{
    Task<List<PermissionsDto>> GetAsync(Guid userId,Guid tenantId, bool isAdmin);
    void Set(Guid userId, List<PermissionsDto> branches);
    void Invalidate(Guid userId);
}
