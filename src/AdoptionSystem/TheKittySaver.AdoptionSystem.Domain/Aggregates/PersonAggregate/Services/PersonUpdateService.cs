using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using Email = TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Email;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

internal sealed class PersonUpdateService : IPersonUpdateService
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonUniquenessCheckerService _personUniquenessCheckerService;

    public PersonUpdateService(
        IPersonRepository personRepository,
        IPersonUniquenessCheckerService personUniquenessCheckerService)
    {
        _personRepository = personRepository;
        _personUniquenessCheckerService = personUniquenessCheckerService;
    }

    public async Task<Result> UpdateEmailAsync(
        PersonId personId,
        Email updatedEmail,
        CancellationToken cancellationToken = default)
    {
        Maybe<Person> maybePerson = await _personRepository.GetByIdAsync(personId, cancellationToken);
        if (maybePerson.HasNoValue)
        {
            return Result.Failure(DomainErrors.PersonAggregatePersonEntity.NotFound(personId));
        }

        if (await _personUniquenessCheckerService.IsEmailTakenAsync(updatedEmail, personId, cancellationToken))
        {
            return Result.Failure(DomainErrors.PersonAggregatePersonEntity.EmailProperty.AlreadyTaken(updatedEmail));
        }
        
        Result updateEmailResult = maybePerson.Value.UpdateEmail(updatedEmail);
        return updateEmailResult;
    }
    
    public async Task<Result> UpdatePhoneNumberAsync(
        PersonId personId,
        PhoneNumber updatedPhoneNumber,
        CancellationToken cancellationToken = default)
    {
        Maybe<Person> maybePerson = await _personRepository.GetByIdAsync(personId, cancellationToken);
        if (maybePerson.HasNoValue)
        {
            return Result.Failure(DomainErrors.PersonAggregatePersonEntity.NotFound(personId));
        }

        if (await _personUniquenessCheckerService.IsPhoneNumberTakenAsync(updatedPhoneNumber, personId, cancellationToken))
        {
            return Result.Failure(DomainErrors.PersonAggregatePersonEntity.PhoneNumberProperty.AlreadyTaken(updatedPhoneNumber));
        }
        
        Result updatePhoneNumberResult = maybePerson.Value.UpdatePhoneNumber(updatedPhoneNumber);
        return updatePhoneNumberResult;
    }
}