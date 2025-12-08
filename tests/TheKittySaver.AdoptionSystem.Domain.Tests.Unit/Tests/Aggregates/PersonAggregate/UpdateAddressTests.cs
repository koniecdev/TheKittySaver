using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

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
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Address.Name).ToLowerInvariant());
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateDetails_WhenValidDataAreProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressPostalCode newPostalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion newRegion = AddressFactory.CreateFixedRegion();
        AddressCity newCity = AddressFactory.CreateRandomCity(Faker);
        AddressLine newLine = AddressFactory.CreateRandomLine(Faker);

        //Act
        Result result = address.UpdateDetails(
            specification,
            newPostalCode,
            newRegion,
            newCity,
            Maybe<AddressLine>.From(newLine));

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.PostalCode.ShouldBe(newPostalCode);
        address.Region.ShouldBe(newRegion);
        address.City.ShouldBe(newCity);
        address.Line.ShouldBe(newLine);
    }

    [Fact]
    public void UpdateDetails_ShouldRemoveLine_WhenMaybeLineIsNone()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker, includeLine: true);
        address.Line.ShouldNotBeNull();
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressPostalCode newPostalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion newRegion = AddressFactory.CreateFixedRegion();
        AddressCity newCity = AddressFactory.CreateRandomCity(Faker);

        //Act
        Result result = address.UpdateDetails(
            specification,
            newPostalCode,
            newRegion,
            newCity,
            Maybe<AddressLine>.None);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        address.Line.ShouldBeNull();
    }

    [Fact]
    public void UpdateDetails_ShouldThrow_WhenNullSpecificationIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action updateDetails = () => address.UpdateDetails(
            null!,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        updateDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("specification");
    }

    [Fact]
    public void UpdateDetails_ShouldThrow_WhenNullPostalCodeIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action updateDetails = () => address.UpdateDetails(
            specification,
            null!,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        updateDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("postalcode");
    }

    [Fact]
    public void UpdateDetails_ShouldThrow_WhenNullRegionIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action updateDetails = () => address.UpdateDetails(
            specification,
            postalCode,
            null!,
            city,
            Maybe<AddressLine>.None);

        //Assert
        updateDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("region");
    }

    [Fact]
    public void UpdateDetails_ShouldThrow_WhenNullCityIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();

        //Act
        Action updateDetails = () => address.UpdateDetails(
            specification,
            postalCode,
            region,
            null!,
            Maybe<AddressLine>.None);

        //Assert
        updateDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("city");
    }

    [Fact]
    public void UpdateDetails_ShouldThrow_WhenNullMaybeLineIsProvided()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action updateDetails = () => address.UpdateDetails(
            specification,
            postalCode,
            region,
            city,
            null!);

        //Assert
        updateDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("maybeline");
    }

    [Fact]
    public void UpdateDetails_ShouldReturnFailure_WhenPostalCodeDoesNotMatchRegion()
    {
        //Arrange
        Address address = AddressFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();

        Result<AddressPostalCode> mismatchedPostalCodeResult = AddressPostalCode.Create("00-001");
        mismatchedPostalCodeResult.EnsureSuccess();

        Result<AddressRegion> regionResult = AddressRegion.Create("Wielkopolskie");
        regionResult.EnsureSuccess();

        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Result result = address.UpdateDetails(
            specification,
            mismatchedPostalCodeResult.Value,
            regionResult.Value,
            city,
            Maybe<AddressLine>.None);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(DomainErrors.AddressConsistency.PostalCodeRegionMismatchCode);
    }
}
