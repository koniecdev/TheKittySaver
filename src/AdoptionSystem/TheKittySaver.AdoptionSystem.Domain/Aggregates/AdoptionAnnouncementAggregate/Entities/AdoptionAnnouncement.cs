using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;

public sealed class AdoptionAnnouncement
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