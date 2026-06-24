using Common.Contracts.authentication.dtos;
using Common.permissions;

namespace Common.Contracts.authentication;

public interface IUserPermissionsCacheService
{
    Task<List<PermissionsDto>> GetAsync(Guid userId, bool isAdmin);
    void Set(Guid userId, List<PermissionsDto> branches);
    void Invalidate(Guid userId);
}
