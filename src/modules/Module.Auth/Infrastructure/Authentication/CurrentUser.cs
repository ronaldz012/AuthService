using System.Security.Claims;
using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Microsoft.AspNetCore.Http;

namespace Module.Auth.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUser
{
    private readonly IUserPermissionsCacheService _cache;
    private List<PermissionsDto>? _branches;
    private Dictionary<Guid, string>? _branchNames;
    private readonly Guid _tenantId; // Guardamos el tenantId del token para el caché

    public Guid UserId { get; }
    public string Username { get; }
    public string? Token { get; }
    public bool IsAuthenticated { get; }
    public bool IsAdmin { get; } // <-- Añadido
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
            : Guid.Empty;

        // Extraer el TenantId del token (necesario para tu servicio de caché)
        _tenantId = IsAuthenticated
            ? Guid.TryParse(user?.FindFirst("tenantId")?.Value, out var tGuid)
                ? tGuid
                : Guid.Empty
            : Guid.Empty;

        // Leer el claim "is_admin" y parsearlo a boolean
        IsAdmin = IsAuthenticated 
            && bool.TryParse(user?.FindFirst("is_admin")?.Value, out var admin) 
            && admin;

        Username = user?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
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
        // Ahora le pasamos los parámetros correctos requeridos por tu CacheService
        _branches ??= await _cache.GetAsync(UserId, IsAdmin);
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