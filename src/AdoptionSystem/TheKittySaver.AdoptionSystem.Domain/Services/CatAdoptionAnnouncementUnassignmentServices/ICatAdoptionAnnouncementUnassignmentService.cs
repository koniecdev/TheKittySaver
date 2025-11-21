using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementUnassignmentServices;

internal interface ICatAdoptionAnnouncementUnassignmentService
{
    Task<Result> UnassignCatFromAdoptionAnnouncementAsync(
        Cat cat,
        AdoptionAnnouncement adoptionAnnouncement,
        CancellationToken cancellationToken = default);
}