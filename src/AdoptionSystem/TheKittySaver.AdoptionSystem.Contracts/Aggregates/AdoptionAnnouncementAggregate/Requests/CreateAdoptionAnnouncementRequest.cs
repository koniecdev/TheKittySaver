using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;

public sealed record CreateAdoptionAnnouncementRequest(
    IEnumerable<Guid> CatIds,
    string? Description,
    CountryCode AddressCountryCode,
    string AddressPostalCode,
    string AddressRegion,
    string AddressCity,
    string? AddressLine,
    string Email,
    string PhoneNumber);
