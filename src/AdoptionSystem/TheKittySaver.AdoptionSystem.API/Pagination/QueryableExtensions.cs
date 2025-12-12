using System.Linq.Expressions;
using System.Reflection;

namespace TheKittySaver.AdoptionSystem.API.Pagination;

/// <summary>
/// Extension methods for IQueryable to support dynamic sorting, filtering, and pagination.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies dynamic sorting to a queryable based on column name and direction.
    /// </summary>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortColumn,
        string? sortOrder = "asc")
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return query;
        }

        var property = GetPropertyInfo<T>(sortColumn);
        if (property is null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var isDescending = sortOrder?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;
        var methodName = isDescending ? "OrderByDescending" : "OrderBy";

        var orderByCall = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), property.PropertyType],
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(orderByCall);
    }

    /// <summary>
    /// Applies then-by sorting to an already ordered queryable.
    /// </summary>
    public static IOrderedQueryable<T> ThenApplySort<T>(
        this IOrderedQueryable<T> query,
        string? sortColumn,
        string? sortOrder = "asc")
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return query;
        }

        var property = GetPropertyInfo<T>(sortColumn);
        if (property is null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var isDescending = sortOrder?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;
        var methodName = isDescending ? "ThenByDescending" : "ThenBy";

        var orderByCall = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), property.PropertyType],
            query.Expression,
            Expression.Quote(orderByExpression));

        return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(orderByCall);
    }

    /// <summary>
    /// Applies filter criteria to a queryable using the provided property filters.
    /// </summary>
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        string? filterString,
        IEnumerable<IPropertyFilter<T>> filters)
    {
        var criteria = FilterCriteria.Parse(filterString);

        if (criteria.Count == 0)
        {
            return query;
        }

        var filterList = filters.ToList();

        foreach (var criterion in criteria)
        {
            var filter = filterList.FirstOrDefault(f =>
                f.PropertyName.Equals(criterion.PropertyName, StringComparison.OrdinalIgnoreCase));

            if (filter is not null)
            {
                query = filter.Apply(query, criterion);
            }
        }

        return query;
    }

    /// <summary>
    /// Applies a global search term across multiple string properties.
    /// </summary>
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? searchTerm,
        params Expression<Func<T, string?>>[] searchProperties)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchProperties.Length == 0)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var searchTermConstant = Expression.Constant(searchTerm.ToLower());

        Expression? combinedExpression = null;

        foreach (var propertySelector in searchProperties)
        {
            // Replace parameter in the property selector
            var propertyBody = new ParameterReplacer(propertySelector.Parameters[0], parameter)
                .Visit(propertySelector.Body);

            // Create: property != null && property.ToLower().Contains(searchTerm)
            var notNullCheck = Expression.NotEqual(propertyBody, Expression.Constant(null, typeof(string)));
            var toLowerCall = Expression.Call(propertyBody, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
            var containsCall = Expression.Call(toLowerCall, typeof(string).GetMethod("Contains", [typeof(string)])!, searchTermConstant);
            var safeContains = Expression.AndAlso(notNullCheck, containsCall);

            combinedExpression = combinedExpression is null
                ? safeContains
                : Expression.OrElse(combinedExpression, safeContains);
        }

        if (combinedExpression is null)
        {
            return query;
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Applies pagination to a queryable.
    /// </summary>
    public static IQueryable<T> ApplyPaging<T>(
        this IQueryable<T> query,
        int offset,
        int limit)
    {
        return query.Skip(offset).Take(limit);
    }

    /// <summary>
    /// Applies all paged query parameters (search, sort, pagination) to a queryable.
    /// </summary>
    public static IQueryable<T> ApplyPagedQuery<T>(
        this IQueryable<T> query,
        IPagedQuery pagedQuery,
        params Expression<Func<T, string?>>[] searchProperties)
    {
        return query
            .ApplySearch(pagedQuery.SearchTerm, searchProperties)
            .ApplySort(pagedQuery.SortColumn, pagedQuery.SortOrder)
            .ApplyPaging(pagedQuery.Offset, pagedQuery.Limit);
    }

    private static PropertyInfo? GetPropertyInfo<T>(string propertyName)
    {
        return typeof(T).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}
