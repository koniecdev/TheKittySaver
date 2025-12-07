using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;

public sealed record CreatePersonRequest(
    IdentityId IdentityId,
    string Username,
    string Email,
    string PhoneNumber);
