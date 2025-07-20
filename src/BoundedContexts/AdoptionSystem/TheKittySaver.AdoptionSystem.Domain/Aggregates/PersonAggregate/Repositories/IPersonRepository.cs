using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;

public interface IPersonRepository : IRepository<Person, PersonId>
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