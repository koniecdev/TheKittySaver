using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;

public sealed record CatDetailsResponse(
    CatId Id,
    PersonId PersonId,
    AdoptionAnnouncementId? AdoptionAnnouncementId,
    string Name,
    string Description,
    int Age,
    CatGenderType Gender,
    ColorType Color,
    decimal WeightValueInKilograms,
    HealthStatusType HealthStatus,
    bool SpecialNeedsStatusHasSpecialNeeds,
    string? SpecialNeedsStatusDescription,
    SpecialNeedsSeverityType SpecialNeedsStatusSeverityType,
    TemperamentType Temperament,
    int AdoptionHistoryReturnCount,
    DateTimeOffset? AdoptionHistoryLastReturnDate,
    string? AdoptionHistoryLastReturnReason,
    ListingSourceType ListingSourceType,
    string ListingSourceSourceName,
    bool IsNeutered,
    FivStatus InfectiousDiseaseStatusFivStatus,
    FelvStatus InfectiousDiseaseStatusFelvStatus,
    DateOnly InfectiousDiseaseStatusLastTestedAt,
    IReadOnlyCollection<CatVaccinationEmbeddedDto> Vaccinations,
    IReadOnlyCollection<CatGalleryItemEmbeddedDto> GalleryItems) : ILinksResponse
{
    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
}
