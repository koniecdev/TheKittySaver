using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using Email = TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Email;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

public interface IPersonUpdateService
{
    Task<Result> UpdateEmailAsync(
        PersonId personId,
        Email updatedEmail,
        CancellationToken cancellationToken = default);

    Task<Result> UpdatePhoneNumberAsync(
        PersonId personId,
        PhoneNumber updatedPhoneNumber,
        CancellationToken cancellationToken = default);
}