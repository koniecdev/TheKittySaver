namespace TheKittySaver.AdoptionSystem.API.Pagination;

/// <summary>
/// Represents a paginated list of items.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public sealed class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Total { get; }
    public int Offset { get; }
    public int Limit { get; }
    public bool HasPreviousPage => Offset > 0;
    public bool HasNextPage => Offset + Items.Count < Total;
    public int TotalPages => Limit > 0 ? (int)Math.Ceiling(Total / (double)Limit) : 0;
    public int CurrentPage => Limit > 0 ? (Offset / Limit) + 1 : 1;
    public PaginationLinks Links { get; }

    private PagedList(
        IReadOnlyList<T> items,
        int total,
        int offset,
        int limit,
        string baseUrl)
    {
        Items = items;
        Total = total;
        Offset = offset;
        Limit = limit;
        Links = new PaginationLinks(baseUrl, offset, limit, total);
    }

    public static PagedList<T> Create(
        IReadOnlyList<T> items,
        int total,
        int offset,
        int limit,
        string baseUrl = "")
    {
        return new PagedList<T>(items, total, offset, limit, baseUrl);
    }

    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source,
        int offset,
        int limit,
        string baseUrl = "",
        CancellationToken cancellationToken = default)
    {
        var total = source.Count();
        var items = source.Skip(offset).Take(limit).ToList();

        return new PagedList<T>(items, total, offset, limit, baseUrl);
    }
}

/// <summary>
/// Contains pagination navigation links.
/// </summary>
public sealed class PaginationLinks
{
    public string? Self { get; }
    public string? First { get; }
    public string? Last { get; }
    public string? Previous { get; }
    public string? Next { get; }

    public PaginationLinks(string baseUrl, int offset, int limit, int total)
    {
        if (string.IsNullOrEmpty(baseUrl) || limit <= 0)
        {
            return;
        }

        var separator = baseUrl.Contains('?') ? "&" : "?";

        Self = $"{baseUrl}{separator}offset={offset}&limit={limit}";
        First = $"{baseUrl}{separator}offset=0&limit={limit}";

        var lastOffset = Math.Max(0, ((total - 1) / limit) * limit);
        Last = $"{baseUrl}{separator}offset={lastOffset}&limit={limit}";

        if (offset > 0)
        {
            var prevOffset = Math.Max(0, offset - limit);
            Previous = $"{baseUrl}{separator}offset={prevOffset}&limit={limit}";
        }

        if (offset + limit < total)
        {
            var nextOffset = offset + limit;
            Next = $"{baseUrl}{separator}offset={nextOffset}&limit={limit}";
        }
    }
}

/// <summary>
/// Response wrapper for paginated data.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public sealed record PagedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Total { get; init; }
    public required int Offset { get; init; }
    public required int Limit { get; init; }
    public required bool HasPreviousPage { get; init; }
    public required bool HasNextPage { get; init; }
    public required int TotalPages { get; init; }
    public required int CurrentPage { get; init; }
    public PaginationLinks? Links { get; init; }

    public static PagedResponse<T> FromPagedList(PagedList<T> pagedList)
    {
        return new PagedResponse<T>
        {
            Items = pagedList.Items,
            Total = pagedList.Total,
            Offset = pagedList.Offset,
            Limit = pagedList.Limit,
            HasPreviousPage = pagedList.HasPreviousPage,
            HasNextPage = pagedList.HasNextPage,
            TotalPages = pagedList.TotalPages,
            CurrentPage = pagedList.CurrentPage,
            Links = pagedList.Links
        };
    }
}
