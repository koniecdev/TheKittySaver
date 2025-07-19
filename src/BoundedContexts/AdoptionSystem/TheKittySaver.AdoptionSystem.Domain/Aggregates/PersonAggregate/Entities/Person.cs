using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;

public sealed class Person : AggregateRoot<PersonId>
{
    private readonly List<PolishAddress> _polishAddresses = [];
    public IdentityId IdentityId { get; private set; } = IdentityId.Empty;
    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public IReadOnlyList<PolishAddress> PolishAddresses => _polishAddresses.AsReadOnly();

    public Result SetIdentityId(IdentityId identityId)
    {
        Ensure.NotEmpty(identityId.Value);
        if (IdentityId != IdentityId.Empty)
        {
            return Result.Failure(DomainErrors.PersonEntity.IdentityIdProperty.AlreadyHasBeenSet);
        }

        IdentityId = identityId;
        return Result.Success();
    }
    
    public void ChangeUsername(Username username)
    {
        ArgumentNullException.ThrowIfNull(username);
        Username = username;
    }
    
    internal void ChangeEmail(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);
        Email = email;
    }
    
    internal void ChangePhoneNumber(PhoneNumber phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        PhoneNumber = phoneNumber;
    }
    
    internal static Person Create(Username username, Email email, PhoneNumber phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phoneNumber);
        
        PersonId id = PersonId.New();
        Person person = new(id, username, email, phoneNumber);
        return person;
    }

    private Person(
        PersonId id,
        Username username,
        Email email,
        PhoneNumber phoneNumber) : base(id)
    {
        Username = username;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}