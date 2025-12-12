namespace TheKittySaver.AdoptionSystem.API.Pagination;

/// <summary>
/// Interface for queries that support pagination.
/// </summary>
public interface IPagedQuery
{
    /// <summary>
    /// Number of items to skip (zero-based offset).
    /// </summary>
    int Offset { get; }

    /// <summary>
    /// Maximum number of items to return.
    /// </summary>
    int Limit { get; }

    /// <summary>
    /// Optional search term for filtering.
    /// </summary>
    string? SearchTerm { get; }

    /// <summary>
    /// Column name to sort by.
    /// </summary>
    string? SortColumn { get; }

    /// <summary>
    /// Sort direction: 'asc' or 'desc'.
    /// </summary>
    string? SortOrder { get; }
}

/// <summary>
/// Base record for paged queries with default values.
/// </summary>
public record PagedQueryBase : IPagedQuery
{
    public int Offset { get; init; }
    public int Limit { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public string? SortColumn { get; init; }
    public string? SortOrder { get; init; } = "asc";
}

/// <summary>
/// Query parameters for HTTP requests supporting pagination.
/// </summary>
public sealed record PagedQueryParameters
{
    private int _offset;
    private int _limit = 20;

    public int Offset
    {
        get => _offset;
        init => _offset = value < 0 ? 0 : value;
    }

    public int Limit
    {
        get => _limit;
        init => _limit = value switch
        {
            <= 0 => 20,
            > 100 => 100,
            _ => value
        };
    }

    public string? SearchTerm { get; init; }
    public string? SortColumn { get; init; }
    public string? SortOrder { get; init; } = "asc";

    /// <summary>
    /// Optional filter criteria in format: "field-operator-value,field2-operator2-value2"
    /// </summary>
    public string? Filter { get; init; }
}
