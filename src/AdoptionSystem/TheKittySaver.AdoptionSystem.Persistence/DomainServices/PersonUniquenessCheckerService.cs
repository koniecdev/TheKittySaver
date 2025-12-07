using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;

namespace TheKittySaver.AdoptionSystem.Persistence.DomainServices;

internal sealed class PersonUniquenessCheckerService : IPersonUniquenessCheckerService
{
    private readonly IApplicationReadDbContext _readDbContext;

    public PersonUniquenessCheckerService(IApplicationReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<bool> IsEmailTakenAsync(
        Email email,
        CancellationToken cancellationToken = default)
    {
        bool emailTaken = await _readDbContext.Persons
            .AnyAsync(person => person.Email == email.Value, cancellationToken);

        return emailTaken;
    }

    public async Task<bool> IsPhoneNumberTakenAsync(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default)
    {
        bool phoneNumberTaken = await _readDbContext.Persons
            .AnyAsync(person => person.PhoneNumber == phoneNumber.Value, cancellationToken);

        return phoneNumberTaken;
    }
}
