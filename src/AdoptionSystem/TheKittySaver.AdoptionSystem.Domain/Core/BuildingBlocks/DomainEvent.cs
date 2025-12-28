using System.ComponentModel.DataAnnotations.Schema;

namespace TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

[NotMapped]
public abstract record DomainEvent : IDomainEvent;
