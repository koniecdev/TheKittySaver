using System.Linq.Expressions;

namespace TheKittySaver.AdoptionSystem.API.Pagination;

/// <summary>
/// Filter for string properties.
/// </summary>
public sealed class StringPropertyFilter<TEntity> : PropertyFilterBase<TEntity, string?>
{
    private readonly string _propertyName;
    private readonly Expression<Func<TEntity, string?>> _propertySelector;

    public override string PropertyName => _propertyName;
    protected override Expression<Func<TEntity, string?>> PropertySelector => _propertySelector;

    public StringPropertyFilter(string propertyName, Expression<Func<TEntity, string?>> propertySelector)
    {
        _propertyName = propertyName;
        _propertySelector = propertySelector;
    }

    protected override Expression<Func<TEntity, bool>>? BuildPredicate(FilterCriteria criteria)
    {
        var parameter = _propertySelector.Parameters[0];
        var property = _propertySelector.Body;
        var value = criteria.Value;

        Expression body = criteria.Operator switch
        {
            FilterOperator.Equals => Expression.Equal(
                property,
                Expression.Constant(value, typeof(string))),

            FilterOperator.NotEquals => Expression.NotEqual(
                property,
                Expression.Constant(value, typeof(string))),

            FilterOperator.Contains => Expression.Call(
                property,
                typeof(string).GetMethod("Contains", [typeof(string)])!,
                Expression.Constant(value)),

            FilterOperator.StartsWith => Expression.Call(
                property,
                typeof(string).GetMethod("StartsWith", [typeof(string)])!,
                Expression.Constant(value)),

            FilterOperator.EndsWith => Expression.Call(
                property,
                typeof(string).GetMethod("EndsWith", [typeof(string)])!,
                Expression.Constant(value)),

            FilterOperator.IsNull => Expression.Equal(
                property,
                Expression.Constant(null, typeof(string))),

            FilterOperator.IsNotNull => Expression.NotEqual(
                property,
                Expression.Constant(null, typeof(string))),

            FilterOperator.In => BuildInExpression(property, value),

            _ => null!
        };

        return body is not null
            ? Expression.Lambda<Func<TEntity, bool>>(body, parameter)
            : null;
    }

    private static Expression BuildInExpression(Expression property, string value)
    {
        var values = value.Split('|').Select(v => v.Trim()).ToList();
        var containsMethod = typeof(List<string>).GetMethod("Contains", [typeof(string)])!;
        return Expression.Call(
            Expression.Constant(values),
            containsMethod,
            property);
    }
}

/// <summary>
/// Filter for numeric properties (int, long, decimal, double).
/// </summary>
public sealed class NumericPropertyFilter<TEntity, TNumeric> : PropertyFilterBase<TEntity, TNumeric>
    where TNumeric : struct, IComparable<TNumeric>
{
    private readonly string _propertyName;
    private readonly Expression<Func<TEntity, TNumeric>> _propertySelector;

    public override string PropertyName => _propertyName;
    protected override Expression<Func<TEntity, TNumeric>> PropertySelector => _propertySelector;

    public NumericPropertyFilter(string propertyName, Expression<Func<TEntity, TNumeric>> propertySelector)
    {
        _propertyName = propertyName;
        _propertySelector = propertySelector;
    }

    protected override Expression<Func<TEntity, bool>>? BuildPredicate(FilterCriteria criteria)
    {
        if (!TryParseValue(criteria.Value, out var parsedValue))
        {
            return null;
        }

        var parameter = _propertySelector.Parameters[0];
        var property = _propertySelector.Body;
        var valueConstant = Expression.Constant(parsedValue, typeof(TNumeric));

        Expression? body = criteria.Operator switch
        {
            FilterOperator.Equals => Expression.Equal(property, valueConstant),
            FilterOperator.NotEquals => Expression.NotEqual(property, valueConstant),
            FilterOperator.GreaterThan => Expression.GreaterThan(property, valueConstant),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, valueConstant),
            FilterOperator.LessThan => Expression.LessThan(property, valueConstant),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(property, valueConstant),
            _ => null
        };

        return body is not null
            ? Expression.Lambda<Func<TEntity, bool>>(body, parameter)
            : null;
    }

    private static bool TryParseValue(string value, out TNumeric result)
    {
        result = default;

        try
        {
            result = (TNumeric)Convert.ChangeType(value, typeof(TNumeric));
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Filter for nullable numeric properties.
/// </summary>
public sealed class NullableNumericPropertyFilter<TEntity, TNumeric> : PropertyFilterBase<TEntity, TNumeric?>
    where TNumeric : struct, IComparable<TNumeric>
{
    private readonly string _propertyName;
    private readonly Expression<Func<TEntity, TNumeric?>> _propertySelector;

    public override string PropertyName => _propertyName;
    protected override Expression<Func<TEntity, TNumeric?>> PropertySelector => _propertySelector;

    public NullableNumericPropertyFilter(string propertyName, Expression<Func<TEntity, TNumeric?>> propertySelector)
    {
        _propertyName = propertyName;
        _propertySelector = propertySelector;
    }

    protected override Expression<Func<TEntity, bool>>? BuildPredicate(FilterCriteria criteria)
    {
        var parameter = _propertySelector.Parameters[0];
        var property = _propertySelector.Body;

        if (criteria.Operator == FilterOperator.IsNull)
        {
            var body = Expression.Equal(property, Expression.Constant(null, typeof(TNumeric?)));
            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }

        if (criteria.Operator == FilterOperator.IsNotNull)
        {
            var body = Expression.NotEqual(property, Expression.Constant(null, typeof(TNumeric?)));
            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }

        if (!TryParseValue(criteria.Value, out var parsedValue))
        {
            return null;
        }

        var valueConstant = Expression.Constant(parsedValue, typeof(TNumeric?));
        var valueProperty = Expression.Property(property, "Value");

        Expression? bodyExpr = criteria.Operator switch
        {
            FilterOperator.Equals => Expression.Equal(property, valueConstant),
            FilterOperator.NotEquals => Expression.NotEqual(property, valueConstant),
            FilterOperator.GreaterThan => Expression.AndAlso(
                Expression.Property(property, "HasValue"),
                Expression.GreaterThan(valueProperty, Expression.Constant(parsedValue!.Value))),
            FilterOperator.GreaterThanOrEqual => Expression.AndAlso(
                Expression.Property(property, "HasValue"),
                Expression.GreaterThanOrEqual(valueProperty, Expression.Constant(parsedValue!.Value))),
            FilterOperator.LessThan => Expression.AndAlso(
                Expression.Property(property, "HasValue"),
                Expression.LessThan(valueProperty, Expression.Constant(parsedValue!.Value))),
            FilterOperator.LessThanOrEqual => Expression.AndAlso(
                Expression.Property(property, "HasValue"),
                Expression.LessThanOrEqual(valueProperty, Expression.Constant(parsedValue!.Value))),
            _ => null
        };

        return bodyExpr is not null
            ? Expression.Lambda<Func<TEntity, bool>>(bodyExpr, parameter)
            : null;
    }

    private static bool TryParseValue(string value, out TNumeric? result)
    {
        result = null;

        try
        {
            result = (TNumeric)Convert.ChangeType(value, typeof(TNumeric));
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Filter for DateTime properties.
/// </summary>
public sealed class DateTimePropertyFilter<TEntity> : PropertyFilterBase<TEntity, DateTime>
{
    private readonly string _propertyName;
    private readonly Expression<Func<TEntity, DateTime>> _propertySelector;

    public override string PropertyName => _propertyName;
    protected override Expression<Func<TEntity, DateTime>> PropertySelector => _propertySelector;

    public DateTimePropertyFilter(string propertyName, Expression<Func<TEntity, DateTime>> propertySelector)
    {
        _propertyName = propertyName;
        _propertySelector = propertySelector;
    }

    protected override Expression<Func<TEntity, bool>>? BuildPredicate(FilterCriteria criteria)
    {
        if (!DateTime.TryParse(criteria.Value, out var parsedValue))
        {
            return null;
        }

        var parameter = _propertySelector.Parameters[0];
        var property = _propertySelector.Body;
        var valueConstant = Expression.Constant(parsedValue, typeof(DateTime));

        Expression? body = criteria.Operator switch
        {
            FilterOperator.Equals => Expression.Equal(property, valueConstant),
            FilterOperator.NotEquals => Expression.NotEqual(property, valueConstant),
            FilterOperator.GreaterThan => Expression.GreaterThan(property, valueConstant),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, valueConstant),
            FilterOperator.LessThan => Expression.LessThan(property, valueConstant),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(property, valueConstant),
            _ => null
        };

        return body is not null
            ? Expression.Lambda<Func<TEntity, bool>>(body, parameter)
            : null;
    }
}

/// <summary>
/// Filter for boolean properties.
/// </summary>
public sealed class BooleanPropertyFilter<TEntity> : PropertyFilterBase<TEntity, bool>
{
    private readonly string _propertyName;
    private readonly Expression<Func<TEntity, bool>> _propertySelector;

    public override string PropertyName => _propertyName;
    protected override Expression<Func<TEntity, bool>> PropertySelector => _propertySelector;

    public BooleanPropertyFilter(string propertyName, Expression<Func<TEntity, bool>> propertySelector)
    {
        _propertyName = propertyName;
        _propertySelector = propertySelector;
    }

    protected override Expression<Func<TEntity, bool>>? BuildPredicate(FilterCriteria criteria)
    {
        if (!bool.TryParse(criteria.Value, out var parsedValue))
        {
            return null;
        }

        var parameter = _propertySelector.Parameters[0];
        var property = _propertySelector.Body;
        var valueConstant = Expression.Constant(parsedValue, typeof(bool));

        Expression? body = criteria.Operator switch
        {
            FilterOperator.Equals => Expression.Equal(property, valueConstant),
            FilterOperator.NotEquals => Expression.NotEqual(property, valueConstant),
            _ => null
        };

        return body is not null
            ? Expression.Lambda<Func<TEntity, bool>>(body, parameter)
            : null;
    }
}

/// <summary>
/// Filter for Guid properties.
/// </summary>
public sealed class GuidPropertyFilter<TEntity> : PropertyFilterBase<TEntity, Guid>
{
    private readonly string _propertyName;
    private readonly Expression<Func<TEntity, Guid>> _propertySelector;

    public override string PropertyName => _propertyName;
    protected override Expression<Func<TEntity, Guid>> PropertySelector => _propertySelector;

    public GuidPropertyFilter(string propertyName, Expression<Func<TEntity, Guid>> propertySelector)
    {
        _propertyName = propertyName;
        _propertySelector = propertySelector;
    }

    protected override Expression<Func<TEntity, bool>>? BuildPredicate(FilterCriteria criteria)
    {
        if (!Guid.TryParse(criteria.Value, out var parsedValue))
        {
            return null;
        }

        var parameter = _propertySelector.Parameters[0];
        var property = _propertySelector.Body;
        var valueConstant = Expression.Constant(parsedValue, typeof(Guid));

        Expression? body = criteria.Operator switch
        {
            FilterOperator.Equals => Expression.Equal(property, valueConstant),
            FilterOperator.NotEquals => Expression.NotEqual(property, valueConstant),
            _ => null
        };

        return body is not null
            ? Expression.Lambda<Func<TEntity, bool>>(body, parameter)
            : null;
    }
}
