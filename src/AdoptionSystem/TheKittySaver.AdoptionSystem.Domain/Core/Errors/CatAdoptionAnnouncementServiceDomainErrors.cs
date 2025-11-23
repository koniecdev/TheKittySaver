using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class CatAdoptionAnnouncementServiceErrors
    {
        public static Error PersonIdMismatch(
            CatId catId,
            PersonId catPersonId,
            AdoptionAnnouncementId adoptionAnnouncementId,
            PersonId adoptionAnnouncementPersonId)
            => new(
                $"{nameof(CatAdoptionAnnouncementServiceErrors)}.PersonIdMismatch",
                $"Cannot reassign cat with id '{catId.Value}' (PersonId: '{catPersonId.Value}') to adoption announcement with id '{adoptionAnnouncementId.Value}' (PersonId: '{adoptionAnnouncementPersonId.Value}'). The cat and adoption announcement must belong to the same person.",
                TypeOfError.Conflict);

        public static Error InfectiousDiseaseConflict(
            CatId catId,
            AdoptionAnnouncementId adoptionAnnouncementId)
            => new(
                $"{nameof(CatAdoptionAnnouncementServiceErrors)}.InfectiousDiseaseConflict",
                $"Cannot assign cat with id '{catId.Value}' to adoption announcement with id '{adoptionAnnouncementId.Value}'. The cat's infectious disease status is not compatible with the cats already assigned to this announcement.",
                TypeOfError.Conflict);
    }
}
