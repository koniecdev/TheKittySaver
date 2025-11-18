using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Events;

public sealed record AdoptionAnnouncementClaimedDomainEvent(
    AdoptionAnnouncementId AdoptionAnnouncementId,
    AnnouncementStatusType AdoptionAnnouncementStatus,
    ClaimedAt? AdoptionAnnouncementClaimedAt) : DomainEvent;