using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using Person = TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities.Person;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.PersonAggregate;

public sealed class UpdatePersonTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void UpdateUsername_ShouldUpdateUsername_WhenValidUsernameIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        Result<Username> newUsernameResult = Username.Create(Faker.Person.UserName + "_updated");
        newUsernameResult.EnsureSuccess();
        Username newUsername = newUsernameResult.Value;

        //Act
        Result result = person.UpdateUsername(newUsername);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.Username.ShouldBe(newUsername);
    }

    [Fact]
    public void UpdateUsername_ShouldThrow_WhenNullUsernameIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);

        //Act
        Action updateUsername = () => person.UpdateUsername(null!);

        //Assert
        updateUsername.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Person.Username).ToLowerInvariant());
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateEmail_WhenValidEmailIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        Result<Email> newEmailResult = Email.Create($"updated_{Faker.Person.Email}");
        newEmailResult.EnsureSuccess();
        Email newEmail = newEmailResult.Value;

        //Act
        Result result = person.UpdateEmail(newEmail);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.Email.ShouldBe(newEmail);
    }

    [Fact]
    public void UpdateEmail_ShouldThrow_WhenNullEmailIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);

        //Act
        Action updateEmail = () => person.UpdateEmail(null!);

        //Assert
        updateEmail.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Person.Email).ToLowerInvariant());
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldUpdatePhoneNumber_WhenValidPhoneNumberIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        PhoneNumber newPhoneNumber = PhoneNumber.CreateUnsafe(Faker.Phone.PhoneNumber());

        //Act
        Result result = person.UpdatePhoneNumber(newPhoneNumber);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.PhoneNumber.ShouldBe(newPhoneNumber);
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldThrow_WhenNullPhoneNumberIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);

        //Act
        Action updatePhoneNumber = () => person.UpdatePhoneNumber(null!);

        //Assert
        updatePhoneNumber.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Person.PhoneNumber).ToLowerInvariant());
    }
}
