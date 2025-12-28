using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;

internal sealed class CatArchiveDomainService : ICatArchiveDomainService
{
    public Result Archive(Cat cat, ArchivedAt archivedAt)
    {
        ArgumentNullException.ThrowIfNull(cat);

        if (cat.AdoptionAnnouncementId is not null)
        {
            return Result.Failure(DomainErrors.CatEntity.CannotArchiveAssignedCat(cat.Id));
        }

        Result archiveResult = cat.Archive(archivedAt);
        return archiveResult;
    }

    public Result Unarchive(Cat cat)
    {
        ArgumentNullException.ThrowIfNull(cat);

        Result unarchiveResult = cat.Unarchive();
        return unarchiveResult;
    }
}
