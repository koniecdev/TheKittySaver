using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

public interface IPersonUniquenessCheckerService
{
    Task<bool> IsEmailTakenAsync(
        Email email,
        CancellationToken cancellationToken = default);

    Task<bool> IsPhoneNumberTakenAsync(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default);
}
