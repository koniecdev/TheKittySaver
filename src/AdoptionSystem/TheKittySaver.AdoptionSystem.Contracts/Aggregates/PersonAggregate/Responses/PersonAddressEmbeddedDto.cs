using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;

public sealed record PersonAddressEmbeddedDto(
    AddressId Id,
    CountryCode CountryCode,
    string Name,
    string PostalCode,
    string Region,
    string City,
    string? Line);
