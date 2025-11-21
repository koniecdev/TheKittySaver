using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;

public sealed record CatReassignedToAnotherAnnouncementDomainEvent(
    CatId CatId,
    AdoptionAnnouncementId SourceAdoptionAnnouncementId,
    AdoptionAnnouncementId DestinationAdoptionAnnouncementId) : DomainEvent;