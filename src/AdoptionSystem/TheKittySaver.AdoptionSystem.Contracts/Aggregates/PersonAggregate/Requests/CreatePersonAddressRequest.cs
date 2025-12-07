using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;

public sealed record CreatePersonAddressRequest(
    CountryCode TwoLetterIsoCountryCode,
    string Name,
    string PostalCode,
    string Region,
    string City,
    string? Line = null
);
