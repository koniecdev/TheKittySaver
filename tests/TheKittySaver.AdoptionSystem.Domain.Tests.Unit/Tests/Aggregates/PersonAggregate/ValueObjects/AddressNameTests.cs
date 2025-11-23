using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.PersonAggregate.ValueObjects;

public sealed class AddressNameTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueIsProvided()
    {
        //Arrange
        string validName = Faker.Address.StreetName();

        //Act
        Result<AddressName> result = AddressName.Create(validName);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validName);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenMaxLengthValueIsProvided()
    {
        //Arrange
        string validName = Faker.Random.String2(AddressName.MaxLength);

        //Act
        Result<AddressName> result = AddressName.Create(validName);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validName);
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasWhitespaceAround()
    {
        //Arrange
        string name = Faker.Address.StreetName();
        string nameWithWhitespace = $"  {name}  ";

        //Act
        Result<AddressName> result = AddressName.Create(nameWithWhitespace);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(name);
    }

    [Theory]
    [ClassData(typeof(NullOrEmptyData))]
    public void Create_ShouldReturnFailure_WhenNullOrEmptyValueIsProvided(string? value)
    {
        //Arrange & Act
        Result<AddressName> result = AddressName.Create(value!);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.AddressEntity.NameProperty.NullOrEmpty);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueExceedsMaxLength()
    {
        //Arrange
        string tooLongName = Faker.Random.String2(AddressName.MaxLength + 1);

        //Act
        Result<AddressName> result = AddressName.Create(tooLongName);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.AddressEntity.NameProperty.LongerThanAllowed);
    }

    [Fact]
    public void ToString_ShouldReturnExpectedValue_WhenValidAddressNameIsProvided()
    {
        //Arrange
        string validName = Faker.Address.StreetName();
        Result<AddressName> result = AddressName.Create(validName);

        //Act
        string toStringResult = result.Value.ToString();

        //Assert
        toStringResult.ShouldBe(validName);
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqual()
    {
        //Arrange
        string name = Faker.Address.StreetName();
        Result<AddressName> result1 = AddressName.Create(name);
        Result<AddressName> result2 = AddressName.Create(name);

        //Act & Assert
        result1.Value.ShouldBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        //Arrange
        Result<AddressName> result1 = AddressName.Create(Faker.Address.StreetName());
        Result<AddressName> result2 = AddressName.Create(Faker.Address.StreetName() + "_different");

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }
}
