namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;

public sealed record UpdatePersonAddressRequest(
    string Name,
    string PostalCode,
    string Region,
    string City,
    string? Line = null
);
