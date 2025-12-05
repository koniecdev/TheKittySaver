using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.PersonAggregate;

public sealed class PolishAddressValidationTests
{
    private static readonly Faker Faker = new();
    private readonly PolandAddressConsistencySpecification _specification = new();

    [Theory]
    [ClassData(typeof(ValidPolishAddressTestData))]
    public void Create_ShouldCreateAddress_WhenValidPolishDataAreProvided(
        string postalCode,
        string region,
        string city)
    {
        //Arrange & Act
        Address address = CreateAddress(postalCode, region, city);

        //Assert
        address.ShouldNotBeNull();
        address.PostalCode.Value.ShouldBe(postalCode);
        address.Region.Value.ShouldBe(region);
        address.City.Value.ShouldBe(city);
    }

    [Theory]
    [InlineData("60-123", "wielkopolskie", "Poznań")]
    [InlineData("60-123", "Wielkopolskie", "Poznań")]
    [InlineData("60-123", "WIELKOPOLSKIE", "Poznań")]
    public void Create_ShouldCreateAddress_WhenRegionNameIsCaseInsensitive(
        string postalCode,
        string region,
        string city)
    {
        //Arrange & Act
        Address address = CreateAddress(postalCode, region, city);

        //Assert
        address.ShouldNotBeNull();
        address.Region.Value.ShouldBe(region);
    }

    [Theory]
    [InlineData("30-001", "malopolskie")]
    [InlineData("30-001", "małopolskie")]
    [InlineData("30-001", "Małopolskie")]
    public void Create_ShouldCreateAddress_WhenRegionNameHasPolishCharacterVariations(
        string postalCode,
        string region)
    {
        //Arrange & Act
        Address address = CreateAddress(postalCode, region, "Kraków");

        //Assert
        address.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("85-001", "kujawsko-pomorskie")]
    [InlineData("85-001", "kujawskopomorskie")]
    [InlineData("85-001", "Kujawsko-Pomorskie")]
    public void Create_ShouldCreateAddress_WhenRegionNameHasHyphenVariations(
        string postalCode,
        string region)
    {
        //Arrange & Act
        Address address = CreateAddress(postalCode, region, "Bydgoszcz");

        //Assert
        address.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("60-123", "Mazowieckie")]
    [InlineData("30-001", "Śląskie")]
    [InlineData("00-001", "Wielkopolskie")]
    [InlineData("40-001", "Pomorskie")]
    [InlineData("80-001", "Dolnośląskie")]
    public void Create_ShouldFail_WhenPostalCodeDoesNotMatchRegion(
        string postalCode,
        string incorrectRegion)
    {
        //Arrange
        Result<AddressName> nameResult = AddressName.Create(Faker.Address.StreetName());
        nameResult.EnsureSuccess();

        Result<AddressRegion> regionResult = AddressRegion.Create(incorrectRegion);
        regionResult.EnsureSuccess();

        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        cityResult.EnsureSuccess();

        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCode);
        postalCodeResult.EnsureSuccess();

        Result<CreatedAt> createdAtResult = CreatedAt.Create(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        createdAtResult.EnsureSuccess();

        //Act
        Result<Address> result = Address.Create(
            specification: _specification,
            personId: PersonId.New(),
            countryCode: CountryCode.PL,
            name: nameResult.Value,
            postalCode: postalCodeResult.Value,
            region: regionResult.Value,
            city: cityResult.Value,
            maybeLine: Maybe<AddressLine>.None,
            createdAt: createdAtResult.Value);

        //Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.Code.ShouldBe(DomainErrors.AddressConsistency.PostalCodeRegionMismatchCode);
    }

    [Fact]
    public void Create_ShouldFail_WhenWielkopolskiePostalCodeUsedWithŁódzkieRegion()
    {
        //Arrange
        string postalCode = "60-123";
        string region = "Łódzkie";

        Result<AddressName> nameResult = AddressName.Create(Faker.Address.StreetName());
        nameResult.EnsureSuccess();

        Result<AddressRegion> regionResult = AddressRegion.Create(region);
        regionResult.EnsureSuccess();

        Result<AddressCity> cityResult = AddressCity.Create("Łódź");
        cityResult.EnsureSuccess();

        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCode);
        postalCodeResult.EnsureSuccess();

        Result<CreatedAt> createdAtResult = CreatedAt.Create(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        createdAtResult.EnsureSuccess();

        //Act
        Result<Address> result = Address.Create(
            specification: _specification,
            personId: PersonId.New(),
            countryCode: CountryCode.PL,
            name: nameResult.Value,
            postalCode: postalCodeResult.Value,
            region: regionResult.Value,
            city: cityResult.Value,
            maybeLine: Maybe<AddressLine>.None,
            createdAt: createdAtResult.Value);

        //Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void Create_ShouldFail_WhenMazowieckiePostalCodeUsedWithMałopolskieRegion()
    {
        //Arrange
        string postalCode = "00-001";
        string region = "Małopolskie";

        Result<AddressName> nameResult = AddressName.Create(Faker.Address.StreetName());
        nameResult.EnsureSuccess();

        Result<AddressRegion> regionResult = AddressRegion.Create(region);
        regionResult.EnsureSuccess();

        Result<AddressCity> cityResult = AddressCity.Create("Kraków");
        cityResult.EnsureSuccess();

        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCode);
        postalCodeResult.EnsureSuccess();

        Result<CreatedAt> createdAtResult = CreatedAt.Create(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        createdAtResult.EnsureSuccess();

        //Act
        Result<Address> result = Address.Create(
            specification: _specification,
            personId: PersonId.New(),
            countryCode: CountryCode.PL,
            name: nameResult.Value,
            postalCode: postalCodeResult.Value,
            region: regionResult.Value,
            city: cityResult.Value,
            maybeLine: Maybe<AddressLine>.None,
            createdAt: createdAtResult.Value);

        //Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateDetails_WhenValidMatchingDataAreProvided()
    {
        //Arrange
        Address address = CreateAddress("60-123", "Wielkopolskie", "Poznań");

        Result<AddressPostalCode> newPostalCodeResult = AddressPostalCode.Create("61-234");
        newPostalCodeResult.EnsureSuccess();

        Result<AddressRegion> newRegionResult = AddressRegion.Create("Wielkopolskie");
        newRegionResult.EnsureSuccess();

        Result<AddressCity> newCityResult = AddressCity.Create("Gniezno");
        newCityResult.EnsureSuccess();

        Result<AddressLine> newLineResult = AddressLine.Create("ul. Nowa 10");
        newLineResult.EnsureSuccess();

        //Act
        Result result = address.UpdateDetails(
            specification: _specification,
            postalCode: newPostalCodeResult.Value,
            region: newRegionResult.Value,
            city: newCityResult.Value,
            maybeLine: Maybe<AddressLine>.From(newLineResult.Value));

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.PostalCode.Value.ShouldBe("61-234");
        address.Region.Value.ShouldBe("Wielkopolskie");
        address.City.Value.ShouldBe("Gniezno");
        address.Line.ShouldNotBeNull();
        address.Line!.Value.ShouldBe("ul. Nowa 10");
    }

    [Fact]
    public void UpdateDetails_ShouldFail_WhenPostalCodeDoesNotMatchRegion()
    {
        //Arrange
        Address address = CreateAddress("60-123", "Wielkopolskie", "Poznań");

        Result<AddressPostalCode> newPostalCodeResult = AddressPostalCode.Create("30-001");
        newPostalCodeResult.EnsureSuccess();

        Result<AddressRegion> newRegionResult = AddressRegion.Create("Wielkopolskie");
        newRegionResult.EnsureSuccess();

        Result<AddressCity> newCityResult = AddressCity.Create("Poznań");
        newCityResult.EnsureSuccess();

        //Act
        Result result = address.UpdateDetails(
            specification: _specification,
            postalCode: newPostalCodeResult.Value,
            region: newRegionResult.Value,
            city: newCityResult.Value,
            maybeLine: Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.Code.ShouldBe(DomainErrors.AddressConsistency.PostalCodeRegionMismatchCode);
        address.PostalCode.Value.ShouldBe("60-123");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateSuccessfully_WhenChangingToMatchingVoivodeship()
    {
        //Arrange
        Address address = CreateAddress("60-123", "Wielkopolskie", "Poznań");

        Result<AddressPostalCode> newPostalCodeResult = AddressPostalCode.Create("30-001");
        newPostalCodeResult.EnsureSuccess();

        Result<AddressRegion> newRegionResult = AddressRegion.Create("Małopolskie");
        newRegionResult.EnsureSuccess();

        Result<AddressCity> newCityResult = AddressCity.Create("Kraków");
        newCityResult.EnsureSuccess();

        //Act
        Result result = address.UpdateDetails(
            specification: _specification,
            postalCode: newPostalCodeResult.Value,
            region: newRegionResult.Value,
            city: newCityResult.Value,
            maybeLine: Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.PostalCode.Value.ShouldBe("30-001");
        address.Region.Value.ShouldBe("Małopolskie");
        address.City.Value.ShouldBe("Kraków");
    }

    [Fact]
    public void UpdateDetails_ShouldRemoveLine_WhenMaybeLineIsNone()
    {
        //Arrange
        Result<AddressLine> lineResult = AddressLine.Create("ul. Stara 5");
        lineResult.EnsureSuccess();
        Address address = CreateAddress("60-123", "Wielkopolskie", "Poznań", lineResult.Value);

        address.Line.ShouldNotBeNull();

        Result<AddressPostalCode> newPostalCodeResult = AddressPostalCode.Create("61-234");
        newPostalCodeResult.EnsureSuccess();

        Result<AddressRegion> newRegionResult = AddressRegion.Create("Wielkopolskie");
        newRegionResult.EnsureSuccess();

        Result<AddressCity> newCityResult = AddressCity.Create("Kalisz");
        newCityResult.EnsureSuccess();

        //Act
        Result result = address.UpdateDetails(
            specification: _specification,
            postalCode: newPostalCodeResult.Value,
            region: newRegionResult.Value,
            city: newCityResult.Value,
            maybeLine: Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.Line.ShouldBeNull();
    }

    [Fact]
    public void UpdateDetails_ShouldThrow_WhenNullSpecificationIsProvided()
    {
        //Arrange
        Address address = CreateAddress("60-123", "Wielkopolskie", "Poznań");

        Result<AddressPostalCode> newPostalCodeResult = AddressPostalCode.Create("61-234");
        newPostalCodeResult.EnsureSuccess();

        Result<AddressRegion> newRegionResult = AddressRegion.Create("Wielkopolskie");
        newRegionResult.EnsureSuccess();

        Result<AddressCity> newCityResult = AddressCity.Create("Gniezno");
        newCityResult.EnsureSuccess();

        //Act
        Action updateDetails = () => address.UpdateDetails(
            specification: null!,
            postalCode: newPostalCodeResult.Value,
            region: newRegionResult.Value,
            city: newCityResult.Value,
            maybeLine: Maybe<AddressLine>.None);

        //Assert
        updateDetails.ShouldThrow<ArgumentNullException>();
    }

    [Theory]
    [InlineData("00-001")]
    [InlineData("99-999")]
    [InlineData("60-000")]
    [InlineData("60-999")]
    public void Create_ShouldCreateAddress_WhenPostalCodeHasBoundaryValues(string postalCode)
    {
        //Arrange
        string region = DetermineCorrectRegionForPostalCode(postalCode);

        //Act
        Address address = CreateAddress(postalCode, region, Faker.Address.City());

        //Assert
        address.ShouldNotBeNull();
        address.PostalCode.Value.ShouldBe(postalCode);
    }

    [Fact]
    public void Create_ShouldCreateAddress_WhenRegionNameHasLeadingAndTrailingSpaces()
    {
        //Arrange
        string postalCode = "60-123";
        string region = "  Wielkopolskie  ";

        Result<AddressName> nameResult = AddressName.Create(Faker.Address.StreetName());
        nameResult.EnsureSuccess();

        Result<AddressRegion> regionResult = AddressRegion.Create(region);
        regionResult.EnsureSuccess();

        Result<AddressCity> cityResult = AddressCity.Create(Faker.Address.City());
        cityResult.EnsureSuccess();

        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCode);
        postalCodeResult.EnsureSuccess();

        Result<CreatedAt> createdAtResult = CreatedAt.Create(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        createdAtResult.EnsureSuccess();

        //Act
        Result<Address> result = Address.Create(
            specification: _specification,
            personId: PersonId.New(),
            countryCode: CountryCode.PL,
            name: nameResult.Value,
            postalCode: postalCodeResult.Value,
            region: regionResult.Value,
            city: cityResult.Value,
            maybeLine: Maybe<AddressLine>.None,
            createdAt: createdAtResult.Value);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Region.Value.ShouldContain("Wielkopolskie");
    }

    private Address CreateAddress(
        string postalCode,
        string region,
        string city,
        AddressLine? line = null)
    {
        Result<AddressName> nameResult = AddressName.Create(Faker.Address.StreetName());
        nameResult.EnsureSuccess();

        Result<AddressRegion> regionResult = AddressRegion.Create(region);
        regionResult.EnsureSuccess();

        Result<AddressCity> cityResult = AddressCity.Create(city);
        cityResult.EnsureSuccess();

        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCode);
        postalCodeResult.EnsureSuccess();

        Maybe<AddressLine> maybeLine = line is not null
            ? Maybe<AddressLine>.From(line)
            : Maybe<AddressLine>.None;

        Result<CreatedAt> createdAtResult = CreatedAt.Create(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        createdAtResult.EnsureSuccess();

        Result<Address> result = Address.Create(
            specification: _specification,
            personId: PersonId.New(),
            countryCode: CountryCode.PL,
            name: nameResult.Value,
            postalCode: postalCodeResult.Value,
            region: regionResult.Value,
            city: cityResult.Value,
            maybeLine: maybeLine,
            createdAt: createdAtResult.Value);

        result.EnsureSuccess();
        return result.Value;
    }

    private static string DetermineCorrectRegionForPostalCode(string postalCode)
    {
        string prefix = postalCode[..2];
        return prefix switch
        {
            "00" or "01" or "02" or "03" or "04" or "05" or "06" or "07" or "08" or "09" or "26" or "27" =>
                "Mazowieckie",
            "10" or "11" or "12" or "13" or "14" => "Warmińsko-Mazurskie",
            "15" or "16" or "17" or "18" or "19" => "Podlaskie",
            "20" or "21" or "22" or "23" or "24" => "Lubelskie",
            "25" or "28" or "29" => "Świętokrzyskie",
            "30" or "31" or "32" or "33" or "34" => "Małopolskie",
            "35" or "36" or "37" or "38" or "39" => "Podkarpackie",
            "40" or "41" or "42" or "43" or "44" => "Śląskie",
            "45" or "46" or "47" or "48" or "49" => "Opolskie",
            "50" or "51" or "52" or "53" or "54" or "55" or "56" or "57" or "58" or "59" => "Dolnośląskie",
            "60" or "61" or "62" or "63" or "64" => "Wielkopolskie",
            "65" or "66" or "67" or "68" or "69" => "Lubuskie",
            "70" or "71" or "72" or "73" or "74" or "75" or "76" or "77" or "78" => "Zachodniopomorskie",
            "80" or "81" or "82" or "83" or "84" => "Pomorskie",
            "85" or "86" or "87" or "88" or "89" => "Kujawsko-Pomorskie",
            "90" or "91" or "92" or "93" or "94" or "95" or "96" or "97" or "98" or "99" => "Łódzkie",
            _ => "Wielkopolskie"
        };
    }
}
