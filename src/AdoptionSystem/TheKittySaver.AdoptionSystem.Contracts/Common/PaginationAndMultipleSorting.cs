namespace TheKittySaver.AdoptionSystem.Contracts.Common;

public interface IPaginationable
{
    int Page { get; }
    int PageSize { get; }
}

public interface ISortable
{
    string? Sort { get; }
}

public sealed record PaginationAndMultipleSorting(int Page = 1, int PageSize = 10, string? Sort = null)
    :  IPaginationable, ISortable;
