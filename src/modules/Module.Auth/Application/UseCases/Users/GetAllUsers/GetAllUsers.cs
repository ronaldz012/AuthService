using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Users.GetAllUsers;

public class GetAllUsers(IAuthDbContext context)
{
    public async Task<Result<PagedResultDto<GetUserResponse>>> execute(UserQueryDto request)
    {
        var query = context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Email))
            query = query.Where(u => u.Email!.Contains(request.Email));

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            query = query.Where(u => u.FirstName.Contains(request.FirstName));

        if (!string.IsNullOrWhiteSpace(request.LastName))
            query = query.Where(u => u.LastName.Contains(request.LastName));

        if (!string.IsNullOrWhiteSpace(request.Username))
            query = query.Where(u => u.Username.Contains(request.Username));

        var (pagedQuery, totalCount) = query.ApplyFilters(request);
        var items = await pagedQuery.Select(x => new GetUserResponse
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
        }).ToListAsync();

        return new PagedResultDto<GetUserResponse>
        {
            Items = items,
            Page = request.GetPageValue(),
            PageSize = request.GetPageSizeValue(),
            TotalCount = totalCount,
        };
    }
    
}