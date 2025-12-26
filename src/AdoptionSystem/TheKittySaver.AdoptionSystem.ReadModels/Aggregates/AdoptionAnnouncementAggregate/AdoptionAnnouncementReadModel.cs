using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;

public sealed record AdoptionAnnouncementReadModel(
    AdoptionAnnouncementId Id,
    PersonId PersonId,
    DateTimeOffset? ClaimedAt,
    string? Description,
    CountryCode AddressCountryCode,
    string AddressPostalCode,
    string AddressRegion,
    string AddressCity,
    string? AddressLine,
    string Email,
    string PhoneNumber,
    AnnouncementStatusType Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ArchivedAt) : IReadOnlyEntity<AdoptionAnnouncementId>
{
    public IReadOnlyList<AdoptionAnnouncementMergeLogReadModel> MergeLogs { get; init; } = [];
    public PersonReadModel Person { get; init; } = null!;
    public IReadOnlyList<CatReadModel> Cats { get; init; } = [];
}
