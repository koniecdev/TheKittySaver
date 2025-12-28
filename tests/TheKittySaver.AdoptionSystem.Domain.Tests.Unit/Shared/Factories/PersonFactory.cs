using Bogus;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
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
        bool replaceIdentityIdWithEmpty = false)
    {
        Result<Username> usernameCreationResult = Username.Create(faker.Person.UserName);
        usernameCreationResult.EnsureSuccess();

        Result<Email> emailCreationResult = Email.Create(faker.Person.Email);
        emailCreationResult.EnsureSuccess();

        PhoneNumber thePhoneNumber = PhoneNumber.CreateUnsafe(faker.Person.Phone);

        IdentityId theIdentityId = IdentityId.Create();

        Result<Person> personCreationResult = Person.Create(
            username: replaceUsernameWithNull ? null! : usernameCreationResult.Value,
            email: replaceEmailWithNull ? null! : emailCreationResult.Value,
            phoneNumber: replacePhoneNumberWithNull ? null! : thePhoneNumber,
            identityId: replaceIdentityIdWithEmpty ? IdentityId.Empty : theIdentityId);
        personCreationResult.EnsureSuccess();

        return personCreationResult.Value;
    }
}
