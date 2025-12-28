using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

internal sealed class PersonArchiveDomainService : IPersonArchiveDomainService
{
    public Result Archive(
        Person person,
        IReadOnlyCollection<Cat> cats,
        IReadOnlyCollection<AdoptionAnnouncement> announcements,
        ArchivedAt archivedAt)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentNullException.ThrowIfNull(cats);
        ArgumentNullException.ThrowIfNull(announcements);
        ArgumentNullException.ThrowIfNull(archivedAt);

        foreach (Cat cat in cats.Where(c => c.AdoptionAnnouncementId is not null))
        {
            cat.UnassignFromAdoptionAnnouncement();
        }

        foreach (Cat cat in cats)
        {
            cat.Archive(archivedAt);
        }

        foreach (AdoptionAnnouncement announcement in announcements)
        {
            announcement.Archive(archivedAt);
        }

        Result archivePersonResult = person.Archive(archivedAt);
        return archivePersonResult;
    }

    public Result Unarchive(
        Person person,
        IReadOnlyCollection<Cat> archivedCats,
        IReadOnlyCollection<AdoptionAnnouncement> archivedAnnouncements)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentNullException.ThrowIfNull(archivedCats);
        ArgumentNullException.ThrowIfNull(archivedAnnouncements);

        Result unarchivePersonResult = person.Unarchive();
        if (unarchivePersonResult.IsFailure)
        {
            return unarchivePersonResult;
        }

        foreach (Cat cat in archivedCats)
        {
            cat.Unarchive();
        }

        foreach (AdoptionAnnouncement announcement in archivedAnnouncements)
        {
            announcement.Unarchive();
        }

        return Result.Success();
    }
}
