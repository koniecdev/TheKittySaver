using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

public interface IPersonCreationService
{
    Task<Result<Person>> CreateAsync(
        Username username,
        Email email,
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default);
}