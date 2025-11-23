using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using Person = TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities.Person;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.PersonAggregate;

public sealed class CreatePersonTests
{
    private static readonly Faker Faker = new();
    
    [Fact]
    public void Create_ShouldCreatePerson_WhenValidDataAreProvided()
    {
        //Arrange & Act
        Person person = PersonFactory.CreateRandom(Faker);
        
        //Assert
        person.ShouldNotBeNull();
        person.Id.ShouldNotBe(PersonId.Empty);
        person.Username.ShouldNotBeNull();
        person.Email.ShouldNotBeNull();
        person.PhoneNumber.ShouldNotBeNull();
        person.CreatedAt.ShouldNotBeNull();
        person.IdentityId.ShouldNotBe(IdentityId.Empty);
        person.Addresses.Count.ShouldBe(0);
    }
    
    [Fact]
    public void Create_ShouldThrow_WhenNullUsernameIsProvided()
    {
        //Arrange & Act
        Func<Person> personCreation = () => PersonFactory.CreateRandom(Faker, replaceUsernameWithNull: true);
        
        //Assert
        personCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(Person.Username).ToLower());
    }
    
    [Fact]
    public void Create_ShouldThrow_WhenNullEmailIsProvided()
    {
        //Arrange & Act
        Func<Person> personCreation = () => PersonFactory.CreateRandom(Faker, replaceEmailWithNull: true);
        
        //Assert
        personCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(Person.Email).ToLower());
    }
    
    [Fact]
    public void Create_ShouldThrow_WhenNullPhoneNumberIsProvided()
    {
        //Arrange & Act
        Func<Person> personCreation = () => PersonFactory.CreateRandom(Faker, replacePhoneNumberWithNull: true);
        
        //Assert
        personCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(Person.PhoneNumber).ToLower());
    }
    
    [Fact]
    public void Create_ShouldThrow_WhenEmptyIdentityIdIsProvided()
    {
        //Arrange & Act
        Func<Person> personCreation = () => PersonFactory.CreateRandom(Faker, replaceIdentityIdWithEmpty: true);
        
        //Assert
        personCreation.ShouldThrow<ArgumentException>()
            .ParamName?.ToLower().ShouldBe(nameof(Person.IdentityId).ToLower());
    }
}