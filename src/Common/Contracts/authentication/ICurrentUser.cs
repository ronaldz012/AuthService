using Common.Contracts.authentication.dtos;
using Common.permissions;

namespace Common.Contracts.authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Username { get; }
    bool IsAdmin { get; }
    string? Token { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<Guid> BranchIds { get; }
    Task<Dictionary<Guid, string>> GetBranchNamesAsync();
    bool HasBranch(Guid branchId);
    Task<List<PermissionsDto>> GetBranchesAsync();
}