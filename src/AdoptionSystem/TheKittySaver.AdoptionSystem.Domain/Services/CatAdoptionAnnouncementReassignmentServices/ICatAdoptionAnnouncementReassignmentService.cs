using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementReassignmentServices;

public interface ICatAdoptionAnnouncementReassignmentService
{
    Result ReassignCatToAnotherAdoptionAnnouncement(
        Cat cat,
        AdoptionAnnouncement sourceAdoptionAnnouncement,
        AdoptionAnnouncement destinationAdoptionAnnouncement,
        IReadOnlyCollection<Cat> catsInitiallyAssignedToDestinationAdoptionAnnouncement,
        DateTimeOffset dateTimeOfOperation);
}