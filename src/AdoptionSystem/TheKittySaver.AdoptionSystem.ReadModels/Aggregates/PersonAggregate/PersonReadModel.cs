using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;

public sealed record PersonReadModel(
    PersonId Id,
    IdentityId IdentityId,
    string Username,
    string Email,
    string PhoneNumber) : IReadOnlyEntity<PersonId>
{
    public IReadOnlyList<AddressReadModel> Addresses { get; init; } = [];
}
