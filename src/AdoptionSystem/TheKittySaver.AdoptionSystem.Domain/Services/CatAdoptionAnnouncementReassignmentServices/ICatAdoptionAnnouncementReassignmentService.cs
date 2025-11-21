using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementReassignmentServices;

internal interface ICatAdoptionAnnouncementReassignmentService
{
    Task<Result> ReassignCatToAnotherAdoptionAnnouncementAsync(
        Cat cat,
        AdoptionAnnouncement sourceAdoptionAnnouncement,
        AdoptionAnnouncement destinationAdoptionAnnouncement,
        DateTimeOffset dateTimeOfOperation,
        CancellationToken cancellationToken = default);
}