using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

public interface IPersonUniquenessCheckerService
{
    Task<bool> IsEmailTakenAsync(
        Email email,
        PersonId? idToExcludeFromSearch = null,
        CancellationToken cancellationToken = default);
    
    Task<bool> IsPhoneNumberTakenAsync(
        PhoneNumber phoneNumber,
        PersonId? idToExcludeFromSearch = null,
        CancellationToken cancellationToken = default);
}