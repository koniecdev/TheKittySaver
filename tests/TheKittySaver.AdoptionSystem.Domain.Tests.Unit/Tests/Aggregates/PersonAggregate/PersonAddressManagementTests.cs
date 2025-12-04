using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
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
    private static readonly DateTimeOffset TestCreatedAtDate = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddAddress_ShouldAddAddress_WhenValidDataAreProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName name = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion region = AddressFactory.CreateRandomRegion(Faker);
        AddressCity city = AddressFactory.CreateRandomCity(Faker);
        AddressLine line = AddressFactory.CreateRandomLine(Faker);
        Result<CreatedAt> createdAtResult = CreatedAt.Create(TestCreatedAtDate);
        createdAtResult.EnsureSuccess();

        //Act
        Result<AddressId> result = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.From(line),
            createdAtResult.Value);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(AddressId.Empty);
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
        AddressPostalCode postalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion region = AddressFactory.CreateRandomRegion(Faker);
        AddressCity city = AddressFactory.CreateRandomCity(Faker);
        Result<CreatedAt> createdAtResult = CreatedAt.Create(TestCreatedAtDate);
        createdAtResult.EnsureSuccess();

        person.AddAddress(specification, CountryCode.PL, name, postalCode, region, city, Maybe<AddressLine>.None, createdAtResult.Value);

        //Act
        Result<AddressId> result = person.AddAddress(
            specification,
            CountryCode.DE,
            name,
            postalCode,
            AddressFactory.CreateRandomRegion(Faker),
            AddressFactory.CreateRandomCity(Faker),
            Maybe<AddressLine>.None,
            createdAtResult.Value);

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
        Result<CreatedAt> createdAtResult = CreatedAt.Create(TestCreatedAtDate);
        createdAtResult.EnsureSuccess();

        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create("60-123");
        postalCodeResult.EnsureSuccess();

        Result<AddressRegion> regionResult = AddressRegion.Create("Mazowieckie");
        regionResult.EnsureSuccess();

        Result<AddressCity> cityResult = AddressCity.Create("Warszawa");
        cityResult.EnsureSuccess();

        //Act
        Result<AddressId> result = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCodeResult.Value,
            regionResult.Value,
            cityResult.Value,
            Maybe<AddressLine>.None,
            createdAtResult.Value);

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
        AddressPostalCode postalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion region = AddressFactory.CreateRandomRegion(Faker);
        AddressCity city = AddressFactory.CreateRandomCity(Faker);
        Result<CreatedAt> createdAtResult = CreatedAt.Create(TestCreatedAtDate);
        createdAtResult.EnsureSuccess();

        //Act
        Action addAddress = () => person.AddAddress(
            specification,
            CountryCode.PL,
            null!,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None,
            createdAtResult.Value);

        //Assert
        addAddress.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe("name");
    }

    [Fact]
    public void UpdateAddressName_ShouldUpdateName_WhenValidDataAreProvided()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();
        AddressName originalName = AddressFactory.CreateRandomName(Faker);
        AddressPostalCode postalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion region = AddressFactory.CreateRandomRegion(Faker);
        AddressCity city = AddressFactory.CreateRandomCity(Faker);
        Result<CreatedAt> createdAtResult = CreatedAt.Create(TestCreatedAtDate);
        createdAtResult.EnsureSuccess();

        Result<AddressId> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            originalName,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None,
            createdAtResult.Value);
        AddressName newName = AddressFactory.CreateRandomName(Faker);

        //Act
        Result result = person.UpdateAddressName(addResult.Value, newName);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.Addresses[0].Name.ShouldBe(newName);
    }

    [Fact]
    public void UpdateAddressName_ShouldReturnFailure_WhenAddressNotFound()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        AddressId nonExistentAddressId = AddressId.New();
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
        AddressPostalCode postalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion region = AddressFactory.CreateRandomRegion(Faker);
        AddressCity city = AddressFactory.CreateRandomCity(Faker);
        Result<CreatedAt> createdAtResult = CreatedAt.Create(TestCreatedAtDate);
        createdAtResult.EnsureSuccess();

        _ = person.AddAddress(
            specification,
            CountryCode.PL,
            firstName,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None,
            createdAtResult.Value);

        Result<AddressId> secondAddressResult = person.AddAddress(
            specification,
            CountryCode.DE,
            secondName,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None,
            createdAtResult.Value);

        //Act
        Result result = person.UpdateAddressName(secondAddressResult.Value, firstName);

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
        AddressPostalCode postalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion region = AddressFactory.CreateRandomRegion(Faker);
        AddressCity city = AddressFactory.CreateRandomCity(Faker);
        Result<CreatedAt> createdAtResult = CreatedAt.Create(TestCreatedAtDate);
        createdAtResult.EnsureSuccess();

        Result<AddressId> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            originalName,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None,
            createdAtResult.Value);

        //Act
        Result result = person.UpdateAddressName(addResult.Value, originalName);

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
        AddressPostalCode originalPostalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion originalRegion = AddressFactory.CreateRandomRegion(Faker);
        AddressCity originalCity = AddressFactory.CreateRandomCity(Faker);
        Result<CreatedAt> createdAtResult = CreatedAt.Create(TestCreatedAtDate);
        createdAtResult.EnsureSuccess();

        Result<AddressId> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            originalPostalCode,
            originalRegion,
            originalCity,
            Maybe<AddressLine>.None,
            createdAtResult.Value);

        AddressPostalCode newPostalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion newRegion = AddressFactory.CreateRandomRegion(Faker);
        AddressCity newCity = AddressFactory.CreateRandomCity(Faker);
        AddressLine newLine = AddressFactory.CreateRandomLine(Faker);

        //Act
        Result result = person.UpdateAddressDetails(
            specification,
            addResult.Value,
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
        AddressId nonExistentAddressId = AddressId.New();
        AddressPostalCode postalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion region = AddressFactory.CreateRandomRegion(Faker);
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
        AddressPostalCode postalCode = AddressFactory.CreateRandomPostalCode(Faker);
        AddressRegion region = AddressFactory.CreateRandomRegion(Faker);
        AddressCity city = AddressFactory.CreateRandomCity(Faker);
        Result<CreatedAt> createdAtResult = CreatedAt.Create(TestCreatedAtDate);
        createdAtResult.EnsureSuccess();

        Result<AddressId> addResult = person.AddAddress(
            specification,
            CountryCode.PL,
            name,
            postalCode,
            region,
            city,
            Maybe<AddressLine>.None,
            createdAtResult.Value);

        //Act
        Result result = person.DeleteAddress(addResult.Value);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        person.Addresses.Count.ShouldBe(0);
    }

    [Fact]
    public void DeleteAddress_ShouldReturnFailure_WhenAddressNotFound()
    {
        //Arrange
        Person person = PersonFactory.CreateRandom(Faker);
        AddressId nonExistentAddressId = AddressId.New();

        //Act
        Result result = person.DeleteAddress(nonExistentAddressId);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AddressEntity.NotFound(nonExistentAddressId));
    }
}