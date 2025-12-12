using System.Linq.Expressions;

namespace TheKittySaver.AdoptionSystem.API.Pagination;

/// <summary>
/// Represents filter criteria parsed from query string.
/// Format: "field-operator-value" (e.g., "name-eq-Whiskers", "age-gt-5")
/// </summary>
public sealed class FilterCriteria
{
    public string PropertyName { get; }
    public FilterOperator Operator { get; }
    public string Value { get; }

    private FilterCriteria(string propertyName, FilterOperator @operator, string value)
    {
        PropertyName = propertyName;
        Operator = @operator;
        Value = value;
    }

    /// <summary>
    /// Parses filter criteria from a query string.
    /// Supports format: "field-operator-value,field2-operator2-value2"
    /// </summary>
    public static IReadOnlyList<FilterCriteria> Parse(string? filterString)
    {
        if (string.IsNullOrWhiteSpace(filterString))
        {
            return [];
        }

        var criteria = new List<FilterCriteria>();
        var filters = filterString.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var filter in filters)
        {
            var parts = filter.Split('-');
            if (parts.Length < 3)
            {
                continue;
            }

            var propertyName = parts[0].Trim();
            var operatorStr = parts[1].Trim().ToLowerInvariant();
            var value = string.Join("-", parts.Skip(2)).Trim();

            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(value))
            {
                continue;
            }

            var filterOperator = ParseOperator(operatorStr);
            if (filterOperator.HasValue)
            {
                criteria.Add(new FilterCriteria(propertyName, filterOperator.Value, value));
            }
        }

        return criteria;
    }

    private static FilterOperator? ParseOperator(string operatorStr)
    {
        return operatorStr switch
        {
            "eq" => FilterOperator.Equals,
            "ne" or "neq" => FilterOperator.NotEquals,
            "gt" => FilterOperator.GreaterThan,
            "gte" or "ge" => FilterOperator.GreaterThanOrEqual,
            "lt" => FilterOperator.LessThan,
            "lte" or "le" => FilterOperator.LessThanOrEqual,
            "contains" or "like" => FilterOperator.Contains,
            "startswith" or "sw" => FilterOperator.StartsWith,
            "endswith" or "ew" => FilterOperator.EndsWith,
            "in" => FilterOperator.In,
            "notin" => FilterOperator.NotIn,
            "isnull" => FilterOperator.IsNull,
            "isnotnull" => FilterOperator.IsNotNull,
            _ => null
        };
    }
}

/// <summary>
/// Supported filter operators.
/// </summary>
public enum FilterOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    In,
    NotIn,
    IsNull,
    IsNotNull
}

/// <summary>
/// Interface for property filters that can apply filtering to a queryable.
/// </summary>
/// <typeparam name="T">The entity type being filtered.</typeparam>
public interface IPropertyFilter<T>
{
    /// <summary>
    /// Applies the filter criteria to the queryable.
    /// </summary>
    IQueryable<T> Apply(IQueryable<T> query, FilterCriteria criteria);

    /// <summary>
    /// Gets the property name this filter handles.
    /// </summary>
    string PropertyName { get; }
}

/// <summary>
/// Base class for property filters.
/// </summary>
/// <typeparam name="TEntity">The entity type being filtered.</typeparam>
/// <typeparam name="TProperty">The property type being filtered.</typeparam>
public abstract class PropertyFilterBase<TEntity, TProperty> : IPropertyFilter<TEntity>
{
    public abstract string PropertyName { get; }

    protected abstract Expression<Func<TEntity, TProperty>> PropertySelector { get; }

    public virtual IQueryable<TEntity> Apply(IQueryable<TEntity> query, FilterCriteria criteria)
    {
        if (!criteria.PropertyName.Equals(PropertyName, StringComparison.OrdinalIgnoreCase))
        {
            return query;
        }

        var predicate = BuildPredicate(criteria);
        return predicate is not null ? query.Where(predicate) : query;
    }

    protected abstract Expression<Func<TEntity, bool>>? BuildPredicate(FilterCriteria criteria);
}
