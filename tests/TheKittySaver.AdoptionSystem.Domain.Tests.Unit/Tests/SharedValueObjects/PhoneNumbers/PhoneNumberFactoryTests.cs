using Bogus;
using NSubstitute;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.SharedValueObjects.PhoneNumbers;

public sealed class PhoneNumberFactoryTests
{
    private static readonly Faker Faker = new();

    private readonly IValidPhoneNumberSpecification _specification;
    private readonly IPhoneNumberNormalizer _normalizer;
    private readonly PhoneNumberFactory _sut;

    public PhoneNumberFactoryTests()
    {
        _specification = Substitute.For<IValidPhoneNumberSpecification>();
        _normalizer = Substitute.For<IPhoneNumberNormalizer>();
        _sut = new PhoneNumberFactory(_specification, _normalizer);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidPhoneNumberIsProvided()
    {
        //Arrange
        string validPhoneNumber = Faker.Phone.PhoneNumber("+48#########");
        _specification.IsSatisfiedBy(validPhoneNumber).Returns(true);
        _normalizer.Normalize(validPhoneNumber).Returns(validPhoneNumber);

        //Act
        Result<PhoneNumber> result = _sut.Create(validPhoneNumber);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validPhoneNumber);
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenPhoneNumberHasWhitespaceAround()
    {
        //Arrange
        string phoneNumber = Faker.Phone.PhoneNumber("+48#########");
        string phoneNumberWithWhitespace = $"  {phoneNumber}  ";
        _specification.IsSatisfiedBy(phoneNumber).Returns(true);
        _normalizer.Normalize(phoneNumber).Returns(phoneNumber);

        //Act
        Result<PhoneNumber> result = _sut.Create(phoneNumberWithWhitespace);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(phoneNumber);
    }

    [Theory]
    [ClassData(typeof(NullOrEmptyData))]
    public void Create_ShouldReturnFailure_WhenNullOrEmptyValueIsProvided(string? value)
    {
        //Arrange & Act
        Result<PhoneNumber> result = _sut.Create(value!);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.PhoneNumberValueObject.NullOrEmpty);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueExceedsMaxLength()
    {
        //Arrange
        string tooLongPhoneNumber = Faker.Random.String2(PhoneNumber.MaxLength + 1, "0123456789");

        //Act
        Result<PhoneNumber> result = _sut.Create(tooLongPhoneNumber);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.PhoneNumberValueObject.LongerThanAllowed);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenSpecificationIsNotSatisfied()
    {
        //Arrange
        string invalidPhoneNumber = Faker.Random.String2(10);
        _specification.IsSatisfiedBy(invalidPhoneNumber).Returns(false);

        //Act
        Result<PhoneNumber> result = _sut.Create(invalidPhoneNumber);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.PhoneNumberValueObject.InvalidFormat);
    }

    [Fact]
    public void Create_ShouldCallNormalizer_WhenPhoneNumberIsValid()
    {
        //Arrange
        string validPhoneNumber = Faker.Phone.PhoneNumber("+48#########");
        string normalizedPhoneNumber = Faker.Phone.PhoneNumber("+48 ### ### ###");
        _specification.IsSatisfiedBy(validPhoneNumber).Returns(true);
        _normalizer.Normalize(validPhoneNumber).Returns(normalizedPhoneNumber);

        //Act
        Result<PhoneNumber> result = _sut.Create(validPhoneNumber);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(normalizedPhoneNumber);
        _normalizer.Received(1).Normalize(validPhoneNumber);
    }

    [Fact]
    public void Create_ShouldNotCallNormalizer_WhenPhoneNumberIsInvalid()
    {
        //Arrange
        string invalidPhoneNumber = Faker.Random.String2(10);
        _specification.IsSatisfiedBy(invalidPhoneNumber).Returns(false);

        //Act
        Result<PhoneNumber> result = _sut.Create(invalidPhoneNumber);

        //Assert
        result.IsFailure.ShouldBeTrue();
        _normalizer.DidNotReceive().Normalize(Arg.Any<string>());
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenMaxLengthPhoneNumberIsProvided()
    {
        //Arrange
        string maxLengthPhoneNumber = Faker.Random.String2(PhoneNumber.MaxLength, "0123456789");
        _specification.IsSatisfiedBy(maxLengthPhoneNumber).Returns(true);
        _normalizer.Normalize(maxLengthPhoneNumber).Returns(maxLengthPhoneNumber);

        //Act
        Result<PhoneNumber> result = _sut.Create(maxLengthPhoneNumber);

        //Assert
        result.IsSuccess.ShouldBeTrue();
    }
}
