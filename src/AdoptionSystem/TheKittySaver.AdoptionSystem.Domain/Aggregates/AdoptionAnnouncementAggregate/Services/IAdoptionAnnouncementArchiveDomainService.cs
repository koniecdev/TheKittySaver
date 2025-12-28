using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Services;

public interface IAdoptionAnnouncementArchiveDomainService
{
    Result Archive(AdoptionAnnouncement announcement, ArchivedAt archivedAt);
    Result Unarchive(AdoptionAnnouncement announcement);
}
