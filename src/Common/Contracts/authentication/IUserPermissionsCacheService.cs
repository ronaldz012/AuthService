using module.Auth.dtos.permissions;

namespace module.Auth.interfaces;

public interface IUserPermissionsCacheService
{
    Task<List<PermissionsDto>> GetAsync(Guid userId, bool isAdmin);
    void Set(Guid userId, List<PermissionsDto> branches);
    void Invalidate(Guid userId);
}
