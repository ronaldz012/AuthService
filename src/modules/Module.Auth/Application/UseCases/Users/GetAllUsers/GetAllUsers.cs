using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.GetAllUsers;

public class GetAllUsers(IAuthDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<GetUsersResponse>> Execute(UserQueryDto request)
    {
        var query = context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Filter))
            query = query.Where(u => u.Email!.Contains(request.Filter));

        if (!string.IsNullOrWhiteSpace(request.Filter))
            query = query.Where(u => u.FirstName.Contains(request.Filter));

        if (!string.IsNullOrWhiteSpace(request.Filter))
            query = query.Where(u => u.LastName.Contains(request.Filter));

        if (!string.IsNullOrWhiteSpace(request.Filter))
            query = query.Where(u => u.Username.Contains(request.Filter));

        if(request.IsActive is not null)
            query = query.Where(u => u.IsActive == request.IsActive);

        var (pagedQuery, totalCount) = query.ApplyFilters(request);
        var items = await pagedQuery.Select(x => new GetUser
        {
            Id = x.Id,
            Username = x.Username,
            FullName = x.FirstName + " " + x.LastName,
            Email = x.Email,
            IsAdmin = x.IsAdmin,
            UserType = x.Type,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Status = x.Status,
            IsActive = x.IsActive,
        }).ToListAsync();

        var activeUsers = await context.Users
            .CountAsync(u => u.TenantId == currentUser.TenantId && u.IsActive);

        return new GetUsersResponse
        {
            Items = items,
            Page = request.GetPageValue(),
            PageSize = request.GetPageSizeValue(),
            TotalCount = totalCount,
            ActiveUsers = activeUsers,
        };
    }
    
}