namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;

public sealed record UpdatePersonRequest(
    string Username,
    string Email,
    string PhoneNumber);