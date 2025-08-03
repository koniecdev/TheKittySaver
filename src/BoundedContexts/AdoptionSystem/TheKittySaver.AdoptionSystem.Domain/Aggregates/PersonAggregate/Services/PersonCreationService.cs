using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using Email = TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Email;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

public sealed class PersonCreationService : IPersonCreationService
{
    private readonly IPersonRepository _personRepository;

    public PersonCreationService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<Result<Person>> CreateAsync(
        Username username,
        Email email,
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phoneNumber);
        
        if (await _personRepository.IsEmailTakenAsync(email, cancellationToken: cancellationToken))
        {
            return Result.Failure<Person>(DomainErrors.PersonEntity.EmailProperty.AlreadyTaken(email));
        }

        if (await _personRepository.IsPhoneNumberTakenAsync(phoneNumber, cancellationToken: cancellationToken))
        {
            return Result.Failure<Person>(DomainErrors.PersonEntity.PhoneNumberProperty.AlreadyTaken(phoneNumber));
        }

        Person person = Person.Create(username, email, phoneNumber);
        
        return Result.Success(person);
    }
}