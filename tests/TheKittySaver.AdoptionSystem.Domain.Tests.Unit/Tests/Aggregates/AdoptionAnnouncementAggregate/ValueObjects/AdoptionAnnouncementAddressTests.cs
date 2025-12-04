using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AdoptionAnnouncementAddressTests
{
    private static readonly Faker Faker = new();
    private static readonly IAddressConsistencySpecification Specification = new PolandAddressConsistencySpecification();

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValuesAreProvidedWithLine()
    {
        //Arrange
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);
        Result<AddressLine> lineResult = AddressLine.Create(Faker.Address.StreetAddress());

        //Act
        Result<AdoptionAnnouncementAddress> result = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.CountryCode.ShouldBe(CountryCode.PL);
        result.Value.PostalCode.ShouldBe(postalCodeResult.Value);
        result.Value.Region.ShouldBe(regionResult.Value);
        result.Value.City.ShouldBe(cityResult.Value);
        result.Value.Line.ShouldBe(lineResult.Value);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValuesAreProvidedWithoutLine()
    {
        //Arrange
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);

        //Act
        Result<AdoptionAnnouncementAddress> result = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.CountryCode.ShouldBe(CountryCode.PL);
        result.Value.PostalCode.ShouldBe(postalCodeResult.Value);
        result.Value.Region.ShouldBe(regionResult.Value);
        result.Value.City.ShouldBe(cityResult.Value);
        result.Value.Line.ShouldBeNull();
    }
    
    [Fact]
    public void Create_ShouldThrow_WhenNullCityIsProvided()
    {
        //Arrange
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);

        //Act
        Func<Result<AdoptionAnnouncementAddress>> addressCreation = () => AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            null!,
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
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);

        //Act
        Func<Result<AdoptionAnnouncementAddress>> addressCreation = () => AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            null!,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(AdoptionAnnouncementAddress.Region).ToLower());
    }

    [Fact]
    public void ToString_ShouldReturnExpectedValue_WhenAddressWithLineIsProvided()
    {
        //Arrange
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);
        Result<AddressLine> lineResult = AddressLine.Create(Faker.Address.StreetAddress());
        Result<AdoptionAnnouncementAddress> addressResult = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
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
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);
        Result<AdoptionAnnouncementAddress> addressResult = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
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
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);
        Result<AddressLine> lineResult = AddressLine.Create(Faker.Address.StreetAddress());

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));

        //Act & Assert
        result1.Value.ShouldBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqualWithoutLine()
    {
        //Arrange
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Act & Assert
        result1.Value.ShouldBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenCitiesAreDifferent()
    {
        //Arrange
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> city1Result = AddressCity.Create(Faker.Address.City());
        Result<AddressCity> city2Result = AddressCity.Create(Faker.Address.City() + "_different");
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            city1Result.Value,
            Maybe<AddressLine>.None);

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            city2Result.Value,
            Maybe<AddressLine>.None);

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenRegionsAreDifferent()
    {
        //Arrange
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);
        string region1Value = "Wielkopolskie";
        Result<AddressRegion> region1Result = AddressRegion.Create(region1Value);
        string region2Value = "Mazowieckie";
        Result<AddressRegion> region2Result = AddressRegion.Create(region2Value);

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            region1Result.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        string postalCode2Value = "00-001";
        Result<AddressPostalCode> postalCode2Result = AddressPostalCode.Create(postalCode2Value);

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCode2Result.Value,
            region2Result.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenLinesAreDifferent()
    {
        //Arrange
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);
        Result<AddressLine> line1Result = AddressLine.Create(Faker.Address.StreetAddress());
        Result<AddressLine> line2Result = AddressLine.Create(Faker.Address.StreetAddress() + "_different");

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.From(line1Result.Value));

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.From(line2Result.Value));

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenOneHasLineAndOtherDoesNot()
    {
        //Arrange
        string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);
        Result<AddressLine> lineResult = AddressLine.Create(Faker.Address.StreetAddress());

        Result<AdoptionAnnouncementAddress> result1 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));

        Result<AdoptionAnnouncementAddress> result2 = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenPostalCodeDoesNotMatchRegion()
    {
        //Arrange
        string regionValue = "Mazowieckie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);

        //Act
        Result<AdoptionAnnouncementAddress> result = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.Code.ShouldBe(DomainErrors.AddressConsistency.PostalCodeRegionMismatchCode);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenWielkopolskiePostalCodeUsedWithMalopolskie()
    {
        //Arrange
        string regionValue = "Małopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "62-800";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);

        //Act
        Result<AdoptionAnnouncementAddress> result = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.Code.ShouldBe(DomainErrors.AddressConsistency.PostalCodeRegionMismatchCode);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenLodzkiePostalCodeUsedWithSlaskie()
    {
        //Arrange
        string regionValue = "Śląskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "93-456";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);

        //Act
        Result<AdoptionAnnouncementAddress> result = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.Code.ShouldBe(DomainErrors.AddressConsistency.PostalCodeRegionMismatchCode);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenPomorskiePostalCodeUsedWithZachodniopomorskie()
    {
        //Arrange
        string regionValue = "Zachodniopomorskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        string postalCodeValue = "80-001";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);

        //Act
        Result<AdoptionAnnouncementAddress> result = AdoptionAnnouncementAddress.Create(
            Specification,
            CountryCode.PL,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.Code.ShouldBe(DomainErrors.AddressConsistency.PostalCodeRegionMismatchCode);
    }
}
