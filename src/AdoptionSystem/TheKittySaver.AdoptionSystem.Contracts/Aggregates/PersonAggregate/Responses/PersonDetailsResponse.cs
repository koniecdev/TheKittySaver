using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;

public sealed record PersonDetailsResponse(
    PersonId Id,
    string Username,
    string Email,
    string PhoneNumber,
    IReadOnlyCollection<PersonAddressEmbeddedDto> Addresses) : ILinksResponse
{
    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
}