using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Aggregates.PersonAggregate.ValueObjects;

public class EmailTests
{
    private static readonly Faker<string> EmailGenerator = new Faker<string>()
        .CustomInstantiator(faker => new string(faker.Person.Email));
    
    [Fact]
    public void Create_ShouldReturnCorrectEmail_WhenCorrectEmailIsProvided()
    {
        //Arrange
        string value = EmailGenerator.Generate();

        //Act
        Result<Email> result = Email.Create(value);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<Email>();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe(value);
        value = result.Value.Value;
        value.ShouldBeOfType<string>();
    }
    
    [Fact]
    public void Create_ShouldReturnCorrectEmail_WhenNotTrimmedEmailIsProvided()
    {
        //Arrange
        string generatedEmail = EmailGenerator.Generate();
        string value = $" {generatedEmail} ";

        //Act
        Result<Email> result = Email.Create(value);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<Email>();
        result.Value.Value.ShouldBe(generatedEmail);
    }
    
    [Fact]
    public void Create_ShouldReturnValidationError_WhenTooLongEmailIsProvided()
    {
        //Arrange
        string tooLongValue = new('a', Email.MaxLength + 1);
        
        //Act
        Result<Email> result = Email.Create(tooLongValue);

        //Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(DomainErrors.PersonEntity.EmailValueObject.LongerThanAllowed);
    }
    
    [Theory]
    [ClassData(typeof(NullOrEmptyData))]
    public void Create_ShouldReturnValidationError_WhenNullOrEmptyEmailIsProvided(string invalidEmail)
    {
        //Act
        Result<Email> result = Email.Create(invalidEmail);

        //Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(DomainErrors.PersonEntity.EmailValueObject.NullOrEmpty);
    }
    
    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public void Create_ShouldReturnValidationError_WhenInvalidEmailIsProvided(string invalidEmail)
    {
        //Act
        Result<Email> result = Email.Create(invalidEmail);

        //Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(DomainErrors.PersonEntity.EmailValueObject.InvalidFormat);
    }
}