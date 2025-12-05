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
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Address.Name).ToLowerInvariant());
    }

}
