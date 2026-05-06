using System.Security.Claims;
using Auth.Contracts.Dtos.permissions;
using Auth.Contracts.Dtos.Users;
using Microsoft.AspNetCore.Http;

namespace Auth.Contracts.Interfaces;

public interface ICurrentUser
{
    int UserId { get; }
    string Username { get; }
    string? Token { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<int> BranchIds { get; }
    Task<Dictionary<int, string>> GetBranchNamesAsync();
    bool HasBranch(int branchId);
    Task<List<PermissionsDto>> GetBranchesAsync();
 }
public class CurrentUserService : ICurrentUser
{
    private readonly IUserPermissionsCacheService _cache;
    private List<PermissionsDto>? _branches;
    private Dictionary<int, string>? _branchNames;

    public int UserId { get; }
    public string Username { get; }
    public string? Token { get; }
    public bool IsAuthenticated { get; }
    public IReadOnlyList<int> BranchIds { get; }
    
    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUserPermissionsCacheService cache)
    {
        _cache = cache;

        var context = httpContextAccessor.HttpContext;
        var user = context?.User;

        IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;
        UserId = IsAuthenticated
            ? int.Parse(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0")
            : 0;
        Username = user?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        Token = context?.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        var headerValues = context?.Request.Headers["X-Branch-Id"].ToString();
        BranchIds = string.IsNullOrWhiteSpace(headerValues)
            ? []
            : headerValues.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList()
                .AsReadOnly();
        
    }

    public bool HasBranch(int branchId) => BranchIds.Contains(branchId);
    

    public async Task<List<PermissionsDto>> GetBranchesAsync()
    {
        _branches ??= await _cache.GetAsync(UserId);
        return _branches;
    }


    public async Task<Dictionary<int, string>> GetBranchNamesAsync()
    {
        if (_branchNames is not null) return _branchNames;

        var branches = await GetBranchesAsync(); // reutiliza el cache
        _branchNames = branches.ToDictionary(p => p.BranchId, p => p.BranchName);
        return _branchNames;
    }
}