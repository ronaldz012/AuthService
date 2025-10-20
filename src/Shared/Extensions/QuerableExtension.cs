using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Shared.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Aplica filtros, ordenación y paginación a una consulta IQueryable genérica.
    /// </summary>
    /// <remarks>
    /// Esta es una extensión de método que centraliza la lógica de manipulación de consultas.
    /// Utiliza los parámetros de un DTO de paginación genérico para:
    /// 1. Aplicar filtros de campo (<c>FieldName</c> y <c>FieldValue</c>).
    /// 2. Contar el número total de registros antes de la paginación para el cálculo del total.
    /// 3. Aplicar ordenación (<c>SortBy</c> y <c>SortDirection</c>).
    /// 4. Aplicar paginación (<c>Page</c> y <c>PageSize</c>).
    ///
    /// Este método es útil para reutilizar la lógica de consulta en múltiples endpoints sin
    /// duplicar código.
    /// </remarks>
    /// <typeparam name="TEntity">El tipo de la entidad sobre la cual se aplicará la consulta.</typeparam>
    /// <param name="query">La consulta IQueryable inicial.</param>
    /// <param name="paginationQuery">El DTO que contiene los parámetros para la paginación, filtrado y ordenación.</param>
    /// <returns>
    /// Una tupla que contiene la consulta IQueryable modificada (con filtros, orden y paginación aplicados)
    /// y el conteo total de registros antes de la paginación.
    /// </returns>
    public static (IQueryable<TEntity> Query, int TotalCount) ApplyFilters<TEntity>(
    this IQueryable<TEntity> query,
    GenericPaginationQueryDto paginationQuery)
    where TEntity : class
    {
        query = query.ApplyFieldFilter(paginationQuery);
        var totalCount = query.Count();
        query = query.ApplySorting(paginationQuery);
        query = query.ApplyPagination(paginationQuery);

        return (query, totalCount);
    }

    public static IQueryable<TEntity> ApplyFieldFilter<TEntity>(
        this IQueryable<TEntity> query,
        GenericPaginationQueryDto paginationQuery)
        where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(paginationQuery.FieldName) ||
            string.IsNullOrWhiteSpace(paginationQuery.FieldValue))
        {
            return query;
        }

        var fieldNamePascalCase = char.ToUpper(paginationQuery.FieldName[0]) +
                                 paginationQuery.FieldName.Substring(1).ToLower();
        var fieldValueLower = paginationQuery.FieldValue.ToLower();

        var property = typeof(TEntity).GetProperty(fieldNamePascalCase,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "w");
        var propertyAccess = Expression.Property(parameter, property);

        Expression propertyAsString;
        if (property.PropertyType != typeof(string))
        {
            var toStringMethod = typeof(object).GetMethod(nameof(object.ToString), Type.EmptyTypes);
            propertyAsString = Expression.Call(propertyAccess, toStringMethod!);
        }
        else
        {
            propertyAsString = propertyAccess;
        }

        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
        var propertyToLower = Expression.Call(propertyAsString, toLowerMethod!);
        var constant = Expression.Constant(fieldValueLower, typeof(string));
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        var containsExpression = Expression.Call(propertyToLower, containsMethod!, constant);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);

        return query.Where(lambda);
    }

    public static IQueryable<TEntity> ApplySorting<TEntity>(
        this IQueryable<TEntity> query,
        GenericPaginationQueryDto paginationQuery)
        where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(paginationQuery.SortBy))
        {
            // Ordenación por defecto - busca propiedad Id
            var defaultProperty = typeof(TEntity).GetProperty("Id");
            if (defaultProperty != null)
            {
                var defaultParameter = Expression.Parameter(typeof(TEntity), "x");
                var defaultPropertyAccess = Expression.Property(defaultParameter, defaultProperty);
                var defaultOrderByExp = Expression.Lambda<Func<TEntity, object>>(
                    Expression.Convert(defaultPropertyAccess, typeof(object)), defaultParameter);
                return query.OrderBy(defaultOrderByExp);
            }
            return query;
        }

        var propertyName = char.ToUpper(paginationQuery.SortBy[0]) + paginationQuery.SortBy.Substring(1);
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var propertyAccess = Expression.Property(parameter, propertyName);
        var orderByExp = Expression.Lambda<Func<TEntity, object>>(
            Expression.Convert(propertyAccess, typeof(object)), parameter);

        if (!string.IsNullOrWhiteSpace(paginationQuery.SortDirection) &&
            paginationQuery.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            return query.OrderByDescending(orderByExp);
        }

        return query.OrderBy(orderByExp);
    }

    public static IQueryable<TEntity> ApplyPagination<TEntity>(
        this IQueryable<TEntity> query,
        GenericPaginationQueryDto paginationQuery)
        where TEntity : class
    {
        if (paginationQuery.GetIsPagedValue())
        {
            query = query
                .Skip((paginationQuery.GetPageValue() - 1) * paginationQuery.GetPageSizeValue())
                .Take(paginationQuery.GetPageSizeValue());
        }

        return query;
    }
}