using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;

public sealed record PersonAddressResponse(
    AddressId Id,
    PersonId PersonId,
    CountryCode CountryCode,
    string Name,
    string PostalCode,
    string Region,
    string City,
    string? Line) : ILinksResponse
{
    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
}
