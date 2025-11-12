using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;

internal sealed class CatAdoptionAnnouncementAssignmentService : ICatAdoptionAnnouncementAssignmentService
{
    private readonly ICatRepository _catRepository;

    public CatAdoptionAnnouncementAssignmentService(
        ICatRepository catRepository)
    {
        _catRepository = catRepository;
    }
    
    //Kot jest dopiero co stworzony, i jeszcze nie ma ogłoszenia
    //Kot jest w draft -> atomowo przenosimy do published, oraz tworzymy active ogłoszenie
    //Flow endpoints:
    //POST Cat (wychodzi draft)
    //POST AdoptionAnnouncement {DraftCatId} (wywołuje AACreationService który wywołuje poniższą metodę)
    public async Task<Result> AssignCatToAdoptionAnnouncementAsync(
        Cat cat,
        AdoptionAnnouncement adoptionAnnouncement,
        CancellationToken cancellationToken = default)
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
            return Result.Failure(DomainErrors.CatEntity.UnavailableForPublish);
        }
        
        if (adoptionAnnouncement.Status is not AnnouncementStatusType.Active)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementEntity.UnavailableForAssigning);
        }
        
        IReadOnlyCollection<Cat> catsAlreadyAssignedToAa = await _catRepository
            .GetCatsByAdoptionAnnouncementIdAsync(adoptionAnnouncement.Id, cancellationToken);

        if (catsAlreadyAssignedToAa.Any(c => c.Id == cat.Id))
        {
            return Result.Failure(DomainErrors.CatEntity.AlreadyAssignedToAnnouncement);
        }

        if (!catsAlreadyAssignedToAa.All(c => c.InfectiousDiseaseStatus.IsCompatibleWith(cat.InfectiousDiseaseStatus)))
        {
            return Result.Failure(DomainErrors.CatAdoptionAnnouncementService
                .InfectiousDiseaseConflict(cat.Id, adoptionAnnouncement.Id));
        }
        
        Result<PublishedAt> catPublishedAtResult = PublishedAt.Create(adoptionAnnouncement.CreatedAt.Value);
        if (catPublishedAtResult.IsFailure)
        {
            return catPublishedAtResult;
        }
        
        Result publishCatResult = cat.Publish(catPublishedAtResult.Value);
        if (publishCatResult.IsFailure)
        {
            return publishCatResult;
        }
        
        Result catAssignToAdoptionAnnouncementResult = cat.AssignToAdoptionAnnouncement(adoptionAnnouncement.Id);
        
        return catAssignToAdoptionAnnouncementResult;
    }
    
    //Kot jest przesunięty do innego ogłoszenia
    //Kot jest w published -> zostawiamy go w published, przenosimy do innego ogłoszenia
    //Jeżeli ogłoszenie po przesunięciu nie ma kotów, >usuwamy je<.
    
    //Kot został wycofany z publicznego widoku
    //cofamy kota do draft, zostawiamy aaid, aa archiwizujemy.
    
}