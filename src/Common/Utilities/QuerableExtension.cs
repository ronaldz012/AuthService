namespace Common.Utilities;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> ApplyPagination<TEntity>(
        this IQueryable<TEntity> query,
        PaginationQueryDto paginationQuery)
        where TEntity : class
    {
        return query
            .Skip((paginationQuery.Page - 1) * paginationQuery.PageSize)
            .Take(paginationQuery.PageSize);
    }
}
