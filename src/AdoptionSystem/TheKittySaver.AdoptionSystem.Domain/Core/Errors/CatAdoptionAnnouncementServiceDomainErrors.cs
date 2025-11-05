using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class CatAdoptionAnnouncementService
    {
        public static Error PersonIdMismatch(
            CatId catId,
            PersonId catPersonId,
            AdoptionAnnouncementId adoptionAnnouncementId,
            PersonId adoptionAnnouncementPersonId)
            => new(
                $"{nameof(CatAdoptionAnnouncementService)}.PersonIdMismatch",
                $"Cannot reassign cat with id '{catId.Value}' (PersonId: '{catPersonId.Value}') to adoption announcement with id '{adoptionAnnouncementId.Value}' (PersonId: '{adoptionAnnouncementPersonId.Value}'). The cat and adoption announcement must belong to the same person.");
    }
}
