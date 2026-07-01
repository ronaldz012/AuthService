using System.Security.Claims;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Http;

namespace Module.Auth.Infrastructure.Authentication;




public class CurrentUserService : ICurrentUser
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public Guid BranchId { get; }
    public IReadOnlyList<Guid> BranchIds { get; }
    public string Username { get; }
    public bool IsAdmin { get; }
    public bool IsAuthenticated { get; }
    public string? Token { get; }

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        var user = context?.User;

        IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;

        if (!IsAuthenticated)
        {
            Username = "Anonymous";
            BranchIds = [];
            return;
        }

        UserId = Guid.TryParse(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uGuid) ? uGuid : Guid.Empty;
        TenantId = Guid.TryParse(user?.FindFirst("tenantId")?.Value, out var tGuid) ? tGuid : Guid.Empty;
        Username = user?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        IsAdmin = bool.TryParse(user?.FindFirst("is_admin")?.Value, out var admin) && admin;
        Token = context?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        var branchHeader = context?.Request.Headers["X-Branch-Id"].ToString();
        
        var parsedIds = string.IsNullOrWhiteSpace(branchHeader)
            ? Array.Empty<Guid>()
            : branchHeader.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Guid.TryParse(s.Trim(), out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToArray();

        BranchIds = parsedIds;
        
        // El BranchId operativo siempre será el primero que envíe el cliente (o Empty si no mandó nada)
        BranchId = parsedIds.FirstOrDefault();
    }
}