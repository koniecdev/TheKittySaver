using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using Email = TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Email;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

public sealed class PersonUpdateService : IPersonUpdateService
{
    private readonly IPersonRepository _personRepository;

    public PersonUpdateService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<Result> UpdateEmailAsync(
        PersonId personId,
        Email updatedEmail,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotEmpty(personId);
        ArgumentNullException.ThrowIfNull(updatedEmail);
        
        Maybe<Person> maybePerson = await _personRepository.GetByIdAsync(personId, cancellationToken);
        if (maybePerson.HasNoValue)
        {
            return Result.Failure(DomainErrors.PersonEntity.NotFound(personId));
        }

        if (await _personRepository.IsEmailTakenAsync(updatedEmail, personId, cancellationToken))
        {
            return Result.Failure(DomainErrors.PersonEntity.EmailProperty.AlreadyTaken(updatedEmail));
        }
        
        maybePerson.Value.UpdateEmail(updatedEmail);
        return Result.Success();
    }
    
    public async Task<Result> UpdatePhoneNumberAsync(
        PersonId personId,
        PhoneNumber updatedPhoneNumber,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotEmpty(personId);
        ArgumentNullException.ThrowIfNull(updatedPhoneNumber);
        
        Maybe<Person> maybePerson = await _personRepository.GetByIdAsync(personId, cancellationToken);
        if (maybePerson.HasNoValue)
        {
            return Result.Failure(DomainErrors.PersonEntity.NotFound(personId));
        }

        if (await _personRepository.IsPhoneNumberTakenAsync(updatedPhoneNumber, personId, cancellationToken))
        {
            return Result.Failure(DomainErrors.PersonEntity.PhoneNumberProperty.AlreadyTaken(updatedPhoneNumber));
        }
        
        maybePerson.Value.UpdatePhoneNumber(updatedPhoneNumber);
        return Result.Success();
    }
}