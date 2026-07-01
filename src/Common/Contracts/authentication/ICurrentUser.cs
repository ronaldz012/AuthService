

namespace Common.Contracts.authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid TenantId { get; }
    Guid BranchId { get; } 
    IReadOnlyList<Guid> BranchIds { get; } 
    
    string Username { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
    string? Token { get; }
}