using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Services;

internal sealed class AdoptionAnnouncementArchiveDomainService : IAdoptionAnnouncementArchiveDomainService
{
    public Result Archive(AdoptionAnnouncement announcement, ArchivedAt archivedAt)
    {
        ArgumentNullException.ThrowIfNull(announcement);
        ArgumentNullException.ThrowIfNull(archivedAt);

        Result archiveResult = announcement.Archive(archivedAt);
        return archiveResult;
    }

    public Result Unarchive(AdoptionAnnouncement announcement)
    {
        ArgumentNullException.ThrowIfNull(announcement);

        Result unarchiveResult = announcement.Unarchive();
        return unarchiveResult;
    }
}
