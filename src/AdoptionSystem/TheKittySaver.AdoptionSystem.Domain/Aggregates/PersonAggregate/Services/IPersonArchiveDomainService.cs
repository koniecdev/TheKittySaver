using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

public interface IPersonArchiveDomainService
{
    Result Archive(
        Person person,
        IReadOnlyCollection<Cat> cats,
        IReadOnlyCollection<AdoptionAnnouncement> announcements,
        ArchivedAt archivedAt);

    Result Unarchive(
        Person person,
        IReadOnlyCollection<Cat> archivedCats,
        IReadOnlyCollection<AdoptionAnnouncement> archivedAnnouncements);
}
