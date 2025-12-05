using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.SharedValueObjects;

public sealed class EmailTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidEmailIsProvided()
    {
        //Arrange
        string validEmail = Faker.Person.Email;

        //Act
        Result<Email> result = Email.Create(validEmail);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validEmail);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenMaxLengthEmailIsProvided()
    {
        //Arrange
        const string domain = "@example.com";
        string localPart = Faker.Random.String2(Email.MaxLength - domain.Length);
        string validEmail = $"{localPart}{domain}";

        //Act
        Result<Email> result = Email.Create(validEmail);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validEmail);
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenEmailHasWhitespaceAround()
    {
        //Arrange
        string email = Faker.Person.Email;
        string emailWithWhitespace = $"  {email}  ";

        //Act
        Result<Email> result = Email.Create(emailWithWhitespace);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(email);
    }

    [Theory]
    [ClassData(typeof(NullOrEmptyData))]
    public void Create_ShouldReturnFailure_WhenNullOrEmptyValueIsProvided(string? value)
    {
        //Arrange & Act
        Result<Email> result = Email.Create(value!);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.EmailValueObject.NullOrEmpty);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueExceedsMaxLength()
    {
        //Arrange
        const string domain = "@example.com";
        string localPart = Faker.Random.String2(Email.MaxLength);
        string tooLongEmail = $"{localPart}{domain}";

        //Act
        Result<Email> result = Email.Create(tooLongEmail);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.EmailValueObject.LongerThanAllowed);
    }

    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public void Create_ShouldReturnFailure_WhenInvalidFormatIsProvided(string invalidEmail)
    {
        //Arrange & Act
        Result<Email> result = Email.Create(invalidEmail);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.EmailValueObject.InvalidFormat);
    }

    [Fact]
    public void ToString_ShouldReturnValue_WhenValidEmailIsProvided()
    {
        //Arrange
        string validEmail = Faker.Person.Email;
        Result<Email> result = Email.Create(validEmail);

        //Act
        string toStringResult = result.Value.ToString();

        //Assert
        toStringResult.ShouldBe(validEmail);
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqual()
    {
        //Arrange
        string email = Faker.Person.Email;
        Result<Email> result1 = Email.Create(email);
        Result<Email> result2 = Email.Create(email);

        //Act & Assert
        result1.Value.ShouldBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        //Arrange
        Result<Email> result1 = Email.Create(Faker.Person.Email);
        Result<Email> result2 = Email.Create($"different_{Faker.Person.Email}");

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }
}
