using Common.Contracts.authentication;
using Microsoft.AspNetCore.Http;

namespace Module.Auth.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUser
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public Guid BranchId { get; }
    public IReadOnlyList<Guid> BranchIds { get; }
    public string FullName { get; }
    public string Username { get; }
    public string? ExternalAuthId { get; }
    public int UserType { get; }
    public bool IsAdmin => UserType is 1 or 2;
    public bool IsAuthenticated { get; }
    public string? Token { get; }

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        var userContext = context?.Items[CurrentUserContextKeys.HttpContextKey] as CurrentUserContext;

        IsAuthenticated = userContext is not null;

        if (userContext is null)
        {
            FullName = "";
            Username = "Anonymous";
            BranchIds = [];
            return;
        }

        TenantId = userContext.TenantId;
        UserId = userContext.UserId;
        FullName = userContext.FullName;
        Username = userContext.Username;
        UserType = userContext.UserType;
        ExternalAuthId = userContext.ExternalAuthId;
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
