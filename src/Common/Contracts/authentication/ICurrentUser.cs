

namespace Common.Contracts.authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid TenantId { get; } 
    Guid BranchId { get; } //for most operations
    IReadOnlyList<Guid> BranchIds { get; }  //for some operations like reports
    
    string FullName { get; }
    string Username { get; }
    int UserType { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
    string? Token { get; }
}