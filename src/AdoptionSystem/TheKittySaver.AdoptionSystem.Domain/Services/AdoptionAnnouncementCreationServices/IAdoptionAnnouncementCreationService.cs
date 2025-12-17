using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

namespace TheKittySaver.AdoptionSystem.Domain.Services.AdoptionAnnouncementCreationServices;

public interface IAdoptionAnnouncementCreationService
{
    Result<AdoptionAnnouncement> Create(
        Cat catToAssign,
        AdoptionAnnouncementAddress address,
        Email email,
        PhoneNumber phoneNumber,
        Maybe<AdoptionAnnouncementDescription> description,
        DateTimeOffset dateTimeOfOperation);
}
