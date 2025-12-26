using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementReassignmentServices;

internal sealed class CatAdoptionAnnouncementReassignmentService : ICatAdoptionAnnouncementReassignmentService
{
    public Result ReassignCatToAnotherAdoptionAnnouncement(
        Cat cat,
        AdoptionAnnouncement sourceAdoptionAnnouncement,
        AdoptionAnnouncement destinationAdoptionAnnouncement,
        IReadOnlyCollection<Cat> catsInitiallyAssignedToDestinationAdoptionAnnouncement,
        DateTimeOffset dateTimeOfOperation)
    {
        if (cat.PersonId != destinationAdoptionAnnouncement.PersonId)
        {
            return Result.Failure(DomainErrors.CatEntity.Assignment.CannotReassignToAnotherOwner(cat.Id));
        }
        
        if (sourceAdoptionAnnouncement.Status is not AnnouncementStatusType.Active)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.StatusProperty.CannotReassignCatFromInactiveAnnouncement);
        }
        
        if(destinationAdoptionAnnouncement.Status is not AnnouncementStatusType.Active)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.StatusProperty.CannotReassignCatToInactiveAnnouncement);
        }

        if (catsInitiallyAssignedToDestinationAdoptionAnnouncement.Contains(cat))
        {
            return Result.Failure(
                DomainErrors.CatEntity.Assignment.CannotReassignToSameAnnouncement(cat.Id));
        }

        bool isCatCompatibileWithOthers =
            catsInitiallyAssignedToDestinationAdoptionAnnouncement.All(alreadyAssignedCat =>
                alreadyAssignedCat.InfectiousDiseaseStatus.IsCompatibleWith(cat.InfectiousDiseaseStatus));

        if (!isCatCompatibileWithOthers)
        {
            return Result.Failure(
                DomainErrors.CatEntity.Assignment.IncompatibleInfectiousDiseaseStatus(cat.Id));
        }

        Result reassignmentResult = cat.ReassignToAnotherAdoptionAnnouncement(
            destinationAdoptionAnnouncement.Id,
            dateTimeOfOperation);
        
        return reassignmentResult;
    }
}
