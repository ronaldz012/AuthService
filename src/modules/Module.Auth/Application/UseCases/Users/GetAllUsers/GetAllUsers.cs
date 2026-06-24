using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Users.GetAllUsers;

public class GetAllUsers(IAuthDbContext context)
{
    public async Task<Result<PagedResultDto<UserDetailResponse>>>  execute(UserQueryDto request)
    {
        var query = context.Users.AsQueryable();
        if (request.Email != null)
        {
            query = query.Where(u => u.Email.Contains(request.Email));
        }
        var (pagedQuery, totalCount) = query.ApplyFilters(request);
        var items = await pagedQuery.Select(x => new UserDetailResponse
        {
            Id = x.Id,
            Username = x.Username,
            Email = x.Email,
            FirstName = x.FirstName,
            LastName = x.LastName,
            DeletedAt = x.DeletedAt,
        }).ToListAsync();

        return new PagedResultDto<UserDetailResponse>()
        {
            Items = items,
            Page = request.GetPageValue(),
            PageSize = request.GetPageSizeValue(),
            TotalCount = totalCount

        };

    }
    
}