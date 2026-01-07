using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;

internal sealed class CatAdoptionAnnouncementAssignmentService : ICatAdoptionAnnouncementAssignmentService
{
    public Result AssignCatToAdoptionAnnouncement(
        Cat cat,
        AdoptionAnnouncement adoptionAnnouncement,
        IReadOnlyCollection<Cat> catsAlreadyAssignedToAa,
        DateTimeOffset dateTimeOfOperation)
    {
        if (cat.PersonId != adoptionAnnouncement.PersonId)
        {
            return Result.Failure(DomainErrors.CatAdoptionAnnouncementService.PersonIdMismatch(
                catId: cat.Id,
                catPersonId: cat.PersonId,
                adoptionAnnouncementId: adoptionAnnouncement.Id,
                adoptionAnnouncementPersonId: adoptionAnnouncement.PersonId));
        }

        if (cat.Status is not CatStatusType.Draft)
        {
            return Result.Failure(DomainErrors.CatEntity.StatusProperty.MustBeDraftForAssignment(cat.Id));
        }

        if (adoptionAnnouncement.Status is not AnnouncementStatusType.Active)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementEntity.StatusProperty.UnavailableForAssigning);
        }

        if (catsAlreadyAssignedToAa.Any(c => c.Id == cat.Id))
        {
            return Result.Failure(DomainErrors.CatEntity.Assignment.AlreadyAssignedToAnnouncement(cat.Id));
        }

        if (!catsAlreadyAssignedToAa.All(c => c.InfectiousDiseaseStatus.IsCompatibleWith(cat.InfectiousDiseaseStatus)))
        {
            return Result.Failure(DomainErrors.CatAdoptionAnnouncementService
                .InfectiousDiseaseConflict(cat.Id, adoptionAnnouncement.Id));
        }

        Result catAssignmentToAdoptionAnnouncementResult = cat.AssignToAdoptionAnnouncement(
            adoptionAnnouncement.Id,
            dateTimeOfOperation);

        return catAssignmentToAdoptionAnnouncementResult;
    }
}
