using Mediator;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;

namespace TheKittySaver.AdoptionSystem.API.DomainEventHandlers;

internal sealed record CatClaimedNotification(CatClaimedDomainEvent DomainEvent) : INotification;

internal sealed record CatReassignedToAnotherAnnouncementNotification(
    CatReassignedToAnotherAnnouncementDomainEvent DomainEvent) : INotification;

internal sealed record CatUnassignedFromAnnouncementNotification(
    CatUnassignedFromAnnouncementDomainEvent DomainEvent) : INotification;
