using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

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