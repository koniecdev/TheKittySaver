namespace TheKittySaver.AdoptionSystem.Contracts.Common;

public sealed record PaginationResponse<T> : ILinksResponse
{
    public required IReadOnlyCollection<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }

    public int TotalPages
    {
        get
        {
            if (PageSize == 0)
            {
                return 0;
            }
            int result = (int)Math.Ceiling(TotalCount / (double)PageSize);
            return result;
        }
    }

    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
}
