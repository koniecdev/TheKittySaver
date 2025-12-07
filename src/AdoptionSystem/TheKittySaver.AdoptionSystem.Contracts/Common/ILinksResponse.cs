namespace TheKittySaver.AdoptionSystem.Contracts.Common;

public interface ILinksResponse
{
    IReadOnlyCollection<LinkDto> Links { get; set; }
}
