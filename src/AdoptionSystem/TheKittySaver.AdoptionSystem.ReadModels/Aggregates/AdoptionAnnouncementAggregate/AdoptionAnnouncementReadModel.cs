using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

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
    AnnouncementStatusType Status)
{
    public IReadOnlyList<AdoptionAnnouncementMergeLogReadModel> MergeLogs { get; init; } = [];
}
