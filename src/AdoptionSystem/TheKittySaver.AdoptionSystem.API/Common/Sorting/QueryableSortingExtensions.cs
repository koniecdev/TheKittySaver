using System.Linq.Expressions;
using System.Reflection;

namespace TheKittySaver.AdoptionSystem.API.Common.Sorting;

internal static class QueryableSortingExtensions
{
    public static IOrderedQueryable<T>? ApplySortOrNull<T>(this IQueryable<T> query, string? sortString)
    {
        if (!SortDescriptor.TryParse(sortString, out SortDescriptor? descriptor))
        {
            return null;
        }

        return query.ApplySort(descriptor);
    }

    public static IOrderedQueryable<T>? ApplySort<T>(this IQueryable<T> query, SortDescriptor descriptor)
    {
        Type entityType = typeof(T);
        PropertyInfo? propertyInfo = entityType.GetProperty(
            descriptor.PropertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo is null)
        {
            return null;
        }

        ParameterExpression parameter = Expression.Parameter(entityType, "x");
        MemberExpression propertyAccess = Expression.Property(parameter, propertyInfo);
        LambdaExpression orderByExpression = Expression.Lambda(propertyAccess, parameter);

        string methodName = descriptor.IsDescending ? "OrderByDescending" : "OrderBy";

        MethodCallExpression resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            [entityType, propertyInfo.PropertyType],
            query.Expression,
            Expression.Quote(orderByExpression));

        return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(resultExpression);
    }
}
