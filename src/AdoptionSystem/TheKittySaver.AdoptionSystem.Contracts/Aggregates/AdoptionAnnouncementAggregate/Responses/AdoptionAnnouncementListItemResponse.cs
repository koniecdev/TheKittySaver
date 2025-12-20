using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;

public sealed record AdoptionAnnouncementListItemResponse(
    AdoptionAnnouncementId Id,
    PersonId PersonId,
    string Username,
    decimal PriorityScore,
    string Title,
    string? Description,
    CountryCode AddressCountryCode,
    string AddressPostalCode,
    string AddressRegion,
    string AddressCity,
    string? AddressLine,
    string Email,
    string PhoneNumber,
    AnnouncementStatusType Status
    ) : ILinksResponse
{
    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
}

public sealed record AdoptionAnnouncementDetailsResponse(
    AdoptionAnnouncementId Id,
    PersonId PersonId,
    string Username,
    string? Description,
    CountryCode AddressCountryCode,
    string AddressPostalCode,
    string AddressRegion,
    string AddressCity,
    string? AddressLine,
    string Email,
    string PhoneNumber,
    AnnouncementStatusType Status
) : ILinksResponse
{
    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
}
