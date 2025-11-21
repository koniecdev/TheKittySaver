using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;

public interface ICatAdoptionAnnouncementAssignmentService
{
    Result AssignCatToAdoptionAnnouncement(
        Cat cat,
        AdoptionAnnouncement adoptionAnnouncement,
        IReadOnlyCollection<Cat> catsAlreadyAssignedToAa,
        DateTimeOffset dateTimeOfOperation);
}