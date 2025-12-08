using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.PersonAggregate;

public sealed class CreateAddressTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldCreateAddress_WhenValidDataAreProvided()
    {
        //Arrange & Act
        Address address = AddressFactory.CreateRandom(Faker);

        //Assert
        address.ShouldNotBeNull();
        address.Id.ShouldNotBe(AddressId.Empty);
        address.PersonId.ShouldNotBe(PersonId.Empty);
        address.CountryCode.ShouldBe(CountryCode.PL);
        address.Name.ShouldNotBeNull();
        address.Region.ShouldNotBeNull();
        address.City.ShouldNotBeNull();
        address.Line.ShouldNotBeNull();
    }

    [Fact]
    public void Create_ShouldCreateAddressWithoutLine_WhenLineIsNotProvided()
    {
        //Arrange & Act
        Address address = AddressFactory.CreateRandom(Faker, includeLine: false);

        //Assert
        address.ShouldNotBeNull();
        address.Line.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldThrow_WhenEmptyPersonIdIsProvided()
    {
        //Arrange & Act
        Func<Address> addressCreation = () => AddressFactory.CreateRandom(Faker, replacePersonIdWithEmpty: true);

        //Assert
        addressCreation.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Address.PersonId).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenUnsetCountryCodeIsProvided()
    {
        //Arrange & Act
        Func<Address> addressCreation = () => AddressFactory.CreateRandom(Faker, replaceCountryCodeWithUnset: true);

        //Assert
        addressCreation.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Address.CountryCode).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullNameIsProvided()
    {
        //Arrange & Act
        Func<Address> addressCreation = () => AddressFactory.CreateRandom(Faker, replaceNameWithNull: true);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Address.Name).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullRegionIsProvided()
    {
        //Arrange & Act
        Func<Address> addressCreation = () => AddressFactory.CreateRandom(Faker, replaceRegionWithNull: true);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Address.Region).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullCityIsProvided()
    {
        //Arrange & Act
        Func<Address> addressCreation = () => AddressFactory.CreateRandom(Faker, replaceCityWithNull: true);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Address.City).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullSpecificationIsProvided()
    {
        //Arrange
        PersonId personId = PersonId.New();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action addressCreation = () => Address.Create(
            null!,
            personId,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("specification");
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullPostalCodeIsProvided()
    {
        //Arrange
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        PersonId personId = PersonId.New();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action addressCreation = () => Address.Create(
            specification,
            personId,
            CountryCode.PL,
            name,
            null!,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("postalcode");
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullMaybeLineIsProvided()
    {
        //Arrange
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        PersonId personId = PersonId.New();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action addressCreation = () => Address.Create(
            specification,
            personId,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            null!);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("maybeline");
    }
}
