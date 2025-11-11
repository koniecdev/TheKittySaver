using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;

internal sealed class PersonCreationService : IPersonCreationService
{
    private readonly IPersonUniquenessCheckerService _personUniquenessCheckerService;

    public PersonCreationService(IPersonUniquenessCheckerService personUniquenessCheckerService)
    {
        _personUniquenessCheckerService = personUniquenessCheckerService;
    }

    public async Task<Result<Person>> CreateAsync(
        Username username,
        Email email,
        PhoneNumber phoneNumber,
        CreatedAt createdAt,
        CancellationToken cancellationToken = default)
    {
        if (await _personUniquenessCheckerService.IsEmailTakenAsync(email, cancellationToken: cancellationToken))
        {
            return Result.Failure<Person>(DomainErrors.PersonEntity.EmailAlreadyTaken(email));
        }

        if (await _personUniquenessCheckerService.IsPhoneNumberTakenAsync(phoneNumber, cancellationToken: cancellationToken))
        {
            return Result.Failure<Person>(DomainErrors.PersonEntity.PhoneNumberAlreadyTaken(phoneNumber));
        }

        Result<Person> createPersonResult = Person.Create(username, email, phoneNumber, createdAt);

        return createPersonResult;
    }
}