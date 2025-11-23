using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.PersonAggregate;

public sealed class UpdateAddressTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void UpdateName_ShouldUpdateName_WhenValidNameIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        AddressName newName = AddressFactory.CreateRandomName(Faker);

        //Act
        Result result = address.UpdateName(newName);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.Name.ShouldBe(newName);
    }

    [Fact]
    public void UpdateName_ShouldThrow_WhenNullNameIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);

        //Act
        Action updateName = () => address.UpdateName(null!);

        //Assert
        updateName.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(Address.Name).ToLower());
    }

    [Fact]
    public void UpdateRegion_ShouldUpdateRegion_WhenValidRegionIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        AddressRegion newRegion = AddressFactory.CreateRandomRegion(Faker);

        //Act
        Result result = address.UpdateRegion(newRegion);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.Region.ShouldBe(newRegion);
    }

    [Fact]
    public void UpdateRegion_ShouldThrow_WhenNullRegionIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);

        //Act
        Action updateRegion = () => address.UpdateRegion(null!);

        //Assert
        updateRegion.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(Address.Region).ToLower());
    }

    [Fact]
    public void UpdateCity_ShouldUpdateCity_WhenValidCityIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        AddressCity newCity = AddressFactory.CreateRandomCity(Faker);

        //Act
        Result result = address.UpdateCity(newCity);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.City.ShouldBe(newCity);
    }

    [Fact]
    public void UpdateCity_ShouldThrow_WhenNullCityIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);

        //Act
        Action updateCity = () => address.UpdateCity(null!);

        //Assert
        updateCity.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(Address.City).ToLower());
    }

    [Fact]
    public void UpdateLine_ShouldUpdateLine_WhenValidLineIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        AddressLine newLine = AddressFactory.CreateRandomLine(Faker);
        Maybe<AddressLine> maybeLine = Maybe<AddressLine>.From(newLine);

        //Act
        Result result = address.UpdateLine(maybeLine);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.Line.ShouldBe(newLine);
    }

    [Fact]
    public void UpdateLine_ShouldSetLineToNull_WhenMaybeNoneIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker, includeLine: true);
        Maybe<AddressLine> maybeLine = Maybe<AddressLine>.None;

        //Act
        Result result = address.UpdateLine(maybeLine);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.Line.ShouldBeNull();
    }
}
