using System.Security.Claims;
using Auth.Contracts.Dtos.permissions;
using Auth.Contracts.Dtos.Users;
using Microsoft.AspNetCore.Http;

namespace Auth.Contracts.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Username { get; }
    string? Token { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<Guid> BranchIds { get; }
    Task<Dictionary<Guid, string>> GetBranchNamesAsync();
    bool HasBranch(Guid branchId);
    Task<List<PermissionsDto>> GetBranchesAsync();
 }
public class CurrentUserService : ICurrentUser
{
    private readonly IUserPermissionsCacheService _cache;
    private List<PermissionsDto>? _branches;
    private Dictionary<Guid, string>? _branchNames;

    public Guid UserId { get; }
    public string Username { get; }
    public string? Token { get; }
    public bool IsAuthenticated { get; }
    public IReadOnlyList<Guid> BranchIds { get; }
    
    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUserPermissionsCacheService cache)
    {
        _cache = cache;

        var context = httpContextAccessor.HttpContext;
        var user = context?.User;

        IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;
        UserId = IsAuthenticated
            ? Guid.TryParse(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var guid)
                ? guid
                : Guid.Empty
            : Guid.Empty;        Username = user?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        Token = context?.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        var headerValues = context?.Request.Headers["X-Branch-Id"].ToString();
        BranchIds = string.IsNullOrWhiteSpace(headerValues)
            ? []
            : headerValues.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Guid.TryParse(s.Trim(), out var id) ? id : (Guid?)null)
                .Where(id => id.HasValue && id.Value != Guid.Empty)
                .Select(id => id!.Value)
                .ToList()
                .AsReadOnly();
    }

    public bool HasBranch(Guid branchId) => BranchIds.Contains(branchId);
    

    public async Task<List<PermissionsDto>> GetBranchesAsync()
    {
        _branches ??= await _cache.GetAsync(UserId);
        return _branches;
    }


    public async Task<Dictionary<Guid, string>> GetBranchNamesAsync()
    {
        if (_branchNames is not null) return _branchNames;

        var branches = await GetBranchesAsync(); // reutiliza el cache
        _branchNames = branches.ToDictionary(p => p.BranchId, p => p.BranchName);
        return _branchNames;
    }
}