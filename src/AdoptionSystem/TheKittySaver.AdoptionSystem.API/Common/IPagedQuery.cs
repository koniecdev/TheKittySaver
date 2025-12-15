namespace TheKittySaver.AdoptionSystem.API.Common;

internal interface IPagedQuery
{
    public int Page { get; }
    public int PageSize { get; }
}
