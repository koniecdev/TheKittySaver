using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

public sealed record CatReadModel(
    CatId Id,
    PersonId PersonId,
    AdoptionAnnouncementId? AdoptionAnnouncementId,
    DateTimeOffset? ClaimedAt,
    DateTimeOffset? PublishedAt,
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
    bool NeuteringStatusIsNeutered,
    FivStatus InfectiousDiseaseStatusFivStatus,
    FelvStatus InfectiousDiseaseStatusFelvStatus,
    DateOnly InfectiousDiseaseStatusLastTestedAt,
    CatStatusType Status)
{
    public CatThumbnailReadModel? Thumbnail { get; init; }
    public IReadOnlyList<CatGalleryItemReadModel> GalleryItems { get; init; } = [];
    public IReadOnlyList<VaccinationReadModel> Vaccinations { get; init; } = [];
}
