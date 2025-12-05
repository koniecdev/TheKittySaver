using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
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
        address.CreatedAt.ShouldNotBeNull();
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
    public void Create_ShouldThrow_WhenNullCreatedAtIsProvided()
    {
        //Arrange & Act
        Func<Address> addressCreation = () => AddressFactory.CreateRandom(Faker, replaceCreatedAtWithNull: true);

        //Assert
        addressCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Address.CreatedAt).ToLowerInvariant());
    }
}
