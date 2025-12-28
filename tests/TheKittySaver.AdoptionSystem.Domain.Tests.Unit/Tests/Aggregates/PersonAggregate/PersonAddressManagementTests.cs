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
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;
using Person = TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities.Person;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.PersonAggregate;

public sealed class PersonAddressManagementTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void AddAddress_ShouldAddAddress_WhenValidDataAreProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);
        AddressLine line = AddressFactory.CreateRandomLine(Faker);



        //Act
        Result<Address> result = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.From(line));

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(AddressId.Empty);
        person.Addresses.Count.ShouldBe(1);
        person.Addresses[0].Name.ShouldBe(name);
    }

    [Fact]
    public void AddAddress_ShouldReturnFailure_WhenAddressWithSameNameAlreadyExists()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);



        person.AddAddress(specification, CountryCode.PL, name, postalCode, region, city, Maybe<AddressLine>.None);

        //Act
        Result<Address> result = person.AddAddress(
            specification,
            CountryCode.DE,
            name,
            postalCode,
            AddressFactory.CreateFixedRegion(),
            AddressFactory.CreateRandomCity(Faker),
            Maybe<AddressLine>.None);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AddressEntity.NameAlreadyTaken(name));
    }

    [Fact]
    public void AddAddress_ShouldReturnFailure_WhenAddressValidationFails()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);



        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create("60-123");
        postalCodeResult.EnsureSuccess();

        Result<AddressRegion> regionResult = AddressRegion.Create("Mazowieckie");
        regionResult.EnsureSuccess();

        Result<AddressCity> cityResult = AddressCity.Create("Warszawa");
        cityResult.EnsureSuccess();

        //Act
        Result<Address> result = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(DomainErrors.AddressConsistency.PostalCodeRegionMismatchCode);
        person.Addresses.Count.ShouldBe(0);
    }

    [Fact]
    public void AddAddress_ShouldThrow_WhenNullNameIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);



        //Act
        Action addAddress = () => person.AddAddress(
            specification,
            CountryCode.PL,
            null!,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        addAddress.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("name");
    }

    [Fact]
    public void UpdateAddressName_ShouldUpdateName_WhenValidDataAreProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName originalName = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);



        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            originalName,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);
        AddressName newName = AddressFactory.CreateRandomName(Faker);

        //Act
        Result result = person.UpdateAddressName(addResult.Value.Id, newName);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.Addresses[0].Name.ShouldBe(newName);
    }

    [Fact]
    public void UpdateAddressName_ShouldReturnFailure_WhenAddressNotFound()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        AddressId nonExistentAddressId = AddressId.Create();
        AddressName newName = AddressFactory.CreateRandomName(Faker);

        //Act
        Result result = person.UpdateAddressName(nonExistentAddressId, newName);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AddressEntity.NotFound(nonExistentAddressId));
    }

    [Fact]
    public void UpdateAddressName_ShouldReturnFailure_WhenNameAlreadyTakenByAnotherAddress()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName firstName = AddressFactory.CreateRandomName(Faker);
        AddressName secondName = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);



        _ = person.AddAddress(
            specification,
            CountryCode.PL,
            firstName,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        Result<Address> secondAddressResult = person.AddAddress(
            specification,
            CountryCode.DE,
            secondName,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Act
        Result result = person.UpdateAddressName(secondAddressResult.Value.Id, firstName);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AddressEntity.NameAlreadyTaken(firstName));
    }

    [Fact]
    public void UpdateAddressName_ShouldUpdateName_WhenUpdatingToSameName()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName originalName = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);



        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            originalName,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Act
        Result result = person.UpdateAddressName(addResult.Value.Id, originalName);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.Addresses[0].Name.ShouldBe(originalName);
    }

    [Fact]
    public void UpdateAddressDetails_ShouldUpdateDetails_WhenValidDataAreProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode originalPostalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion originalRegion = AddressFactory.CreateFixedRegion();
        AddressCity originalCity = AddressFactory.CreateRandomCity(Faker);



        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            originalPostalCode,
            originalRegion,
            originalCity,
            Maybe<AddressLine>.None);

        AddressPostalCode newPostalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion newRegion = AddressFactory.CreateFixedRegion();
        AddressCity newCity = AddressFactory.CreateRandomCity(Faker);
        AddressLine newLine = AddressFactory.CreateRandomLine(Faker);

        //Act
        Result result = person.UpdateAddressDetails(
            specification,
            addResult.Value.Id,
            newPostalCode,
            newRegion,
            newCity,
            Maybe<AddressLine>.From(newLine));

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.Addresses[0].PostalCode.ShouldBe(newPostalCode);
        person.Addresses[0].Region.ShouldBe(newRegion);
        person.Addresses[0].City.ShouldBe(newCity);
        person.Addresses[0].Line.ShouldBe(newLine);
    }

    [Fact]
    public void UpdateAddressDetails_ShouldReturnFailure_WhenAddressNotFound()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressId nonExistentAddressId = AddressId.Create();
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Result result = person.UpdateAddressDetails(
            specification,
            nonExistentAddressId,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AddressEntity.NotFound(nonExistentAddressId));
    }

    [Fact]
    public void DeleteAddress_ShouldDeleteAddress_WhenAddressExists()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);



        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Act
        Result result = person.RemoveAddress(addResult.Value.Id);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.Addresses.Count.ShouldBe(0);
    }

    [Fact]
    public void DeleteAddress_ShouldReturnFailure_WhenAddressNotFound()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        AddressId nonExistentAddressId = AddressId.Create();

        //Act
        Result result = person.RemoveAddress(nonExistentAddressId);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AddressEntity.NotFound(nonExistentAddressId));
    }

    [Fact]
    public void DeleteAddress_ShouldThrow_WhenEmptyAddressIdIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);

        //Act
        Action deleteAddress = () => person.RemoveAddress(AddressId.Empty);

        //Assert
        deleteAddress.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldBe("addressid");
    }

    [Fact]
    public void AddAddress_ShouldThrow_WhenNullSpecificationIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action addAddress = () => person.AddAddress(
            null!,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        addAddress.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("specification");
    }

    [Fact]
    public void AddAddress_ShouldThrow_WhenUnsetCountryCodeIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action addAddress = () => person.AddAddress(
            specification,
            CountryCode.Unset,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        addAddress.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldBe("countrycode");
    }

    [Fact]
    public void AddAddress_ShouldThrow_WhenNullPostalCodeIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action addAddress = () => person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            null!,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        addAddress.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("postalcode");
    }

    [Fact]
    public void AddAddress_ShouldThrow_WhenNullRegionIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action addAddress = () => person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            null!,
            city,
            Maybe<AddressLine>.None);

        //Assert
        addAddress.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("region");
    }

    [Fact]
    public void AddAddress_ShouldThrow_WhenNullCityIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();

        //Act
        Action addAddress = () => person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            null!,
            Maybe<AddressLine>.None);

        //Assert
        addAddress.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("city");
    }

    [Fact]
    public void AddAddress_ShouldThrow_WhenNullMaybeLineIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action addAddress = () => person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            null!);

        //Assert
        addAddress.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("maybeline");
    }

    [Fact]
    public void UpdateAddressName_ShouldThrow_WhenEmptyAddressIdIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        AddressName newName = AddressFactory.CreateRandomName(Faker);

        //Act
        Action updateAddressName = () => person.UpdateAddressName(AddressId.Empty, newName);

        //Assert
        updateAddressName.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldBe("addressid");
    }

    [Fact]
    public void UpdateAddressName_ShouldThrow_WhenNullUpdatedNameIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName originalName = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            originalName,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Act
        Action updateAddressName = () => person.UpdateAddressName(addResult.Value.Id, null!);

        //Assert
        updateAddressName.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("updatedname");
    }

    [Fact]
    public void UpdateAddressDetails_ShouldThrow_WhenNullSpecificationIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Act
        Action updateAddressDetails = () => person.UpdateAddressDetails(
            null!,
            addResult.Value.Id,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        updateAddressDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("specification");
    }

    [Fact]
    public void UpdateAddressDetails_ShouldThrow_WhenEmptyAddressIdIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        //Act
        Action updateAddressDetails = () => person.UpdateAddressDetails(
            specification,
            AddressId.Empty,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        updateAddressDetails.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldBe("addressid");
    }

    [Fact]
    public void UpdateAddressDetails_ShouldThrow_WhenNullPostalCodeIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Act
        Action updateAddressDetails = () => person.UpdateAddressDetails(
            specification,
            addResult.Value.Id,
            null!,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        updateAddressDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("postalcode");
    }

    [Fact]
    public void UpdateAddressDetails_ShouldThrow_WhenNullRegionIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Act
        Action updateAddressDetails = () => person.UpdateAddressDetails(
            specification,
            addResult.Value.Id,
            postalCode,
            null!,
            city,
            Maybe<AddressLine>.None);

        //Assert
        updateAddressDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("region");
    }

    [Fact]
    public void UpdateAddressDetails_ShouldThrow_WhenNullCityIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Act
        Action updateAddressDetails = () => person.UpdateAddressDetails(
            specification,
            addResult.Value.Id,
            postalCode,
            region,
            null!,
            Maybe<AddressLine>.None);

        //Assert
        updateAddressDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("city");
    }

    [Fact]
    public void UpdateAddressDetails_ShouldThrow_WhenNullMaybeLineIsProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        //Act
        Action updateAddressDetails = () => person.UpdateAddressDetails(
            specification,
            addResult.Value.Id,
            postalCode,
            region,
            city,
            null!);

        //Assert
        updateAddressDetails.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe("maybeline");
    }

    [Fact]
    public void UpdateAddressDetails_ShouldReturnFailure_WhenValidationFails()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateFixedPostalCode();
        AddressRegion region = AddressFactory.CreateFixedRegion();
        AddressCity city = AddressFactory.CreateRandomCity(Faker);

        Result<Address> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None);

        Result<AddressPostalCode> mismatchedPostalCodeResult = AddressPostalCode.Create("00-001");
        mismatchedPostalCodeResult.EnsureSuccess();

        //Act
        Result result = person.UpdateAddressDetails(
            specification,
            addResult.Value.Id,
            mismatchedPostalCodeResult.Value,
            region,
            city,
            Maybe<AddressLine>.None);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(DomainErrors.AddressConsistency.PostalCodeRegionMismatchCode);
    }
}
