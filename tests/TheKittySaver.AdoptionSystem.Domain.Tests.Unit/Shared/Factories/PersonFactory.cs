using Bogus;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using Person = TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities.Person;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;

internal static class PersonFactory
{
    public static Person CreateRandom(
        Faker faker,
        bool replaceUsernameWithNull = false,
        bool replaceEmailWithNull = false,
        bool replacePhoneNumberWithNull = false,
        bool replaceCreatedAtWithNull = false,
        bool replaceIdentityIdWithEmpty = false)
    {
        Result<Username> usernameCreationResult = Username.Create(faker.Person.UserName);
        usernameCreationResult.EnsureSuccess();
        
        Result<Email> emailCreationResult = Email.Create(faker.Person.Email);
        emailCreationResult.EnsureSuccess();
        
        PhoneNumber thePhoneNumber = PhoneNumber.CreateUnsafe(faker.Person.Phone);
        
        Result<CreatedAt> createdAtCreationResult = CreatedAt.Create(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        createdAtCreationResult.EnsureSuccess();
        
        IdentityId theIdentityId = IdentityId.New();
        
        Result<Person> personCreationResult = Person.Create(
            username: replaceUsernameWithNull ? null! : usernameCreationResult.Value,
            email: replaceEmailWithNull ? null! : emailCreationResult.Value,
            phoneNumber: replacePhoneNumberWithNull ? null! : thePhoneNumber,
            createdAt: replaceCreatedAtWithNull ? null! : createdAtCreationResult.Value,
            identityId: replaceIdentityIdWithEmpty ? IdentityId.Empty : theIdentityId);
        personCreationResult.EnsureSuccess();
        
        return personCreationResult.Value;
    }
}