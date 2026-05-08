using System;
using Auth.Contracts.Dtos.permissions;
using Auth.Contracts.Dtos.Users;

namespace Auth.Contracts.Interfaces;

public interface IUserPermissionsCacheService
{
    Task<List<PermissionsDto>> GetAsync(Guid userId);
    void Set(Guid userId, List<PermissionsDto> branches);
    void Invalidate(Guid userId);
}
