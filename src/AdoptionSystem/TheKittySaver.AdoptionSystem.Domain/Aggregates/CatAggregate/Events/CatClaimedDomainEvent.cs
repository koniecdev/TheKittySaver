using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;

public sealed record CatClaimedDomainEvent(Cat ClaimedCat) : DomainEvent;
