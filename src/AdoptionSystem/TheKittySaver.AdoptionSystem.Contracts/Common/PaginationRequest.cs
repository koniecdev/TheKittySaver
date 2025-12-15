namespace TheKittySaver.AdoptionSystem.Contracts.Common;

public sealed record PaginationRequest(int Page = 1, int PageSize = 10);
