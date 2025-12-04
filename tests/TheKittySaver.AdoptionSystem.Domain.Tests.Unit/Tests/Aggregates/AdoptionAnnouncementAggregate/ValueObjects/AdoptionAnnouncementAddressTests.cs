using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AdoptionAnnouncementAddressTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValuesAreProvidedWithLine()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());
        Result<AddressLine> lineResult = AddressLine.Create(Faker.Address.StreetAddress());

        //Act
        Result<AdoptionAnnouncementAddress> result = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.City.ShouldBe(cityResult.Value);
        result.Value.Region.ShouldBe(regionResult.Value);
        result.Value.Line.ShouldBe(lineResult.Value);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValuesAreProvidedWithoutLine()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());

        //Act
        Result<AdoptionAnnouncementAddress> result = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.City.ShouldBe(cityResult.Value);
        result.Value.Region.ShouldBe(regionResult.Value);
        result.Value.Line.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullCityIsProvided()
    {
        //Arrange
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());

        //Act
        Func<Result<AdoptionAnnouncementAddress>> addressCreation = () => AdoptionAnnouncementAddress.Create(
            null!,
            regionResult.Value,
            Maybe<AddressLine>.None);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(AdoptionAnnouncementAddress.City).ToLower());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullRegionIsProvided()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());

        //Act
        Func<Result<AdoptionAnnouncementAddress>> addressCreation = () => AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            null!,
            Maybe<AddressLine>.None);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(AdoptionAnnouncementAddress.Region).ToLower());
    }

    [Fact]
    public void ToString_ShouldReturnExpectedValue_WhenAddressWithLineIsProvided()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());
        Result<AddressLine> lineResult = AddressLine.Create(Faker.Address.StreetAddress());
        Result<AdoptionAnnouncementAddress> addressResult = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));

        string expectedString = $"{lineResult.Value.Value}, {cityResult.Value.Value}, {regionResult.Value.Value}";

        //Act
        string toStringResult = addressResult.Value.ToString();

        //Assert
        toStringResult.ShouldBe(expectedString);
    }

    [Fact]
    public void ToString_ShouldReturnExpectedValue_WhenAddressWithoutLineIsProvided()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());
        Result<AdoptionAnnouncementAddress> addressResult = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.None);

        string expectedString = $"{cityResult.Value.Value}, {regionResult.Value.Value}";

        //Act
        string toStringResult = addressResult.Value.ToString();

        //Assert
        toStringResult.ShouldBe(expectedString);
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqualWithLine()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());
        Result<AddressLine> lineResult = AddressLine.Create(Faker.Address.StreetAddress());

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));

        //Act & Assert
        result1.Value.ShouldBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqualWithoutLine()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.None);

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.None);

        //Act & Assert
        result1.Value.ShouldBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenCitiesAreDifferent()
    {
        //Arrange
        Result<AddressCity> city1Result = AddressCity.Create(Faker.Address.City());
        Result<AddressCity> city2Result = AddressCity.Create(Faker.Address.City() + "_different");
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            city1Result.Value,
            regionResult.Value,
            Maybe<AddressLine>.None);

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            city2Result.Value,
            regionResult.Value,
            Maybe<AddressLine>.None);

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenRegionsAreDifferent()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        Result<AddressRegion> region1Result = AddressRegion.Create(Faker.Address.State());
        Result<AddressRegion> region2Result = AddressRegion.Create(Faker.Address.State() + "_different");

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            region1Result.Value,
            Maybe<AddressLine>.None);

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            region2Result.Value,
            Maybe<AddressLine>.None);

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenLinesAreDifferent()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());
        Result<AddressLine> line1Result = AddressLine.Create(Faker.Address.StreetAddress());
        Result<AddressLine> line2Result = AddressLine.Create(Faker.Address.StreetAddress() + "_different");

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.From(line1Result.Value));

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.From(line2Result.Value));

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenOneHasLineAndOtherDoesNot()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        Result<AddressRegion> regionResult = AddressRegion.Create(Faker.Address.State());
        Result<AddressLine> lineResult = AddressLine.Create(Faker.Address.StreetAddress());

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.None);

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }
}
