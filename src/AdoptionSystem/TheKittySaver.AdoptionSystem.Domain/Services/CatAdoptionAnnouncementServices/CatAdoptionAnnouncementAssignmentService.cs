using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;

internal sealed class CatAdoptionAnnouncementAssignmentService : ICatAdoptionAnnouncementAssignmentService
{
    
    //Kot jest dopiero co stworzony, i jeszcze nie ma ogłoszenia
    //Kot jest w draft -> atomowo przenosimy do published, oraz tworzymy active ogłoszenie
    //Flow endpoints:
    //POST Cat (wychodzi draft)
    //POST AdoptionAnnouncement {DraftCatId} (wywołuje AACreationService który wywołuje poniższą metodę)

    //Inny scenariusz, kot jest dodany od razu do rodziennego ogłoszenia
    //wtedy też ta metoda jest wystarczająca
    
    //jeszcze innym casem jest przeciągnięcie kota z tego ogłoszenia do prywatnej strefy.
    //tym powinien się zająć unassignment service
    
    //Innym casem jest przeciągnięcie kota z tego ogłoszenia do innego ogłoszenia
    //tym powiniein się zając reassignment service
    
    //teoretycznie jeszcze innym case'm jest oznaczenie kota jako claimed, poza claimem ogłoszenia
    //to teoretycznie ukryje kota w ogłoszneiu, i tam pozostanie.
    //tym powininen zająć się catclaimservice. ReadModele powinny ignorować claim koty w ogłoszeniach do kalkulacji prio
    
    
    //To jest serwis który odpowiada za przypisanie draftowego kota do ogłoszenia już istniejącego, z jakimiś kotami.
    //Scenariusz przypisania kota do nowego ogłoszenia, bo to invariant ogłoszenia że dodaje sie atomowo z pierwszym publishem kota
    //to odpowiedzialność AdoptionAnnouncementCreationService.
    //Ale nalezy pamiętać że user po utworzeniu kota ma go w drafcie. Moze wyjść w etapie tworzenia ogloszenia, albo zdecydować
    //się w ogóle ogłoszenia na razie nie dodawać. Wtedy pierwszą operacją będzie właśnie nowe ogłoszenie kota, albo dopisanie do istniejącego.
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