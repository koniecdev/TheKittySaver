using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;

public sealed class AdoptionAnnouncement : AggregateRoot<AdoptionAnnouncementId>
{
    public PersonId PersonId { get; }
    public Description Description { get; private set; }
    
    internal static AdoptionAnnouncement Create(PersonId personId)
    {
        Ensure.NotEmpty(personId);
        AdoptionAnnouncement instance = new(personId);
        return instance;
    }
    
    private AdoptionAnnouncement(PersonId personId)
    {
        PersonId = personId;
    }
}