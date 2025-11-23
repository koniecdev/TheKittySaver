using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.PersonAggregate.ValueObjects;

public sealed class UsernameTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueIsProvided()
    {
        //Arrange
        string validUsername = Faker.Person.UserName;

        //Act
        Result<Username> result = Username.Create(validUsername);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validUsername);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenMaxLengthValueIsProvided()
    {
        //Arrange
        string validUsername = Faker.Random.String2(Username.MaxLength);

        //Act
        Result<Username> result = Username.Create(validUsername);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validUsername);
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasWhitespaceAround()
    {
        //Arrange
        string username = Faker.Person.UserName;
        string usernameWithWhitespace = $"  {username}  ";

        //Act
        Result<Username> result = Username.Create(usernameWithWhitespace);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(username);
    }

    [Theory]
    [ClassData(typeof(NullOrEmptyData))]
    public void Create_ShouldReturnFailure_WhenNullOrEmptyValueIsProvided(string? value)
    {
        //Arrange & Act
        Result<Username> result = Username.Create(value!);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.PersonEntity.UsernameProperty.NullOrEmpty);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueExceedsMaxLength()
    {
        //Arrange
        string tooLongUsername = Faker.Random.String2(Username.MaxLength + 1);

        //Act
        Result<Username> result = Username.Create(tooLongUsername);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.PersonEntity.UsernameProperty.LongerThanAllowed);
    }

    [Fact]
    public void ToString_ShouldReturnExpectedValue_WhenValidUsernameIsProvided()
    {
        //Arrange
        string validUsername = Faker.Person.UserName;
        Result<Username> result = Username.Create(validUsername);

        //Act
        string toStringResult = result.Value.ToString();

        //Assert
        toStringResult.ShouldBe(validUsername);
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqual()
    {
        //Arrange
        string username = Faker.Person.UserName;
        Result<Username> result1 = Username.Create(username);
        Result<Username> result2 = Username.Create(username);

        //Act & Assert
        result1.Value.ShouldBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        //Arrange
        Result<Username> result1 = Username.Create(Faker.Person.UserName);
        Result<Username> result2 = Username.Create(Faker.Person.UserName + "_different");

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }
}
