using System;
using Auth.Contracts.Dtos.permissions;
using Auth.Contracts.Dtos.Users;

namespace Auth.Contracts.Interfaces;

public interface IUserPermissionsCacheService
{
    Task<List<PermissionsDto>> GetAsync(int userId);
    void Set(int userId, List<PermissionsDto> branches);
    void Invalidate(int userId);
}
