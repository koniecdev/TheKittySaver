namespace TheKittySaver.AdoptionSystem.Contracts.Common;

public sealed record PaginationRequest(int Page = 1, int PageSize = 10)
{
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 10;

    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize < 1 ? DefaultPageSize : PageSize > MaxPageSize ? MaxPageSize : PageSize;
}
