using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;
using TheKittySaver.AdoptionSystem.ReadModels.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;

public sealed record AddressReadModel(
    AddressId Id,
    PersonId PersonId,
    CountryCode CountryCode,
    string Name,
    string PostalCode,
    string Region,
    string City,
    string? Line,
    DateTimeOffset CreatedAt) : IReadOnlyEntity<AddressId>
{
    public PersonReadModel Person { get; init; } = null!;
}
