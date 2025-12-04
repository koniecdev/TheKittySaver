using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AdoptionAnnouncementDescriptionTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueIsProvided()
    {
        //Arrange
        string validDescription = Faker.Lorem.Sentence(10);

        //Act
        Result<AdoptionAnnouncementDescription> result = AdoptionAnnouncementDescription.Create(validDescription);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validDescription);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenMaxLengthValueIsProvided()
    {
        //Arrange
        string validDescription = Faker.Random.String2(AdoptionAnnouncementDescription.MaxLength);

        //Act
        Result<AdoptionAnnouncementDescription> result = AdoptionAnnouncementDescription.Create(validDescription);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validDescription);
    }

    [Theory]
    [ClassData(typeof(NullOrEmptyData))]
    public void Create_ShouldReturnFailure_WhenNullOrEmptyValueIsProvided(string? value)
    {
        //Arrange & Act
        Result<AdoptionAnnouncementDescription> result = AdoptionAnnouncementDescription.Create(value!);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementErrors.DescriptionProperty.NullOrEmpty);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueExceedsMaxLength()
    {
        //Arrange
        string tooLongDescription = Faker.Random.String2(AdoptionAnnouncementDescription.MaxLength + 1);

        //Act
        Result<AdoptionAnnouncementDescription> result = AdoptionAnnouncementDescription.Create(tooLongDescription);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementErrors.DescriptionProperty.LongerThanAllowed);
    }

    [Fact]
    public void ToString_ShouldReturnExpectedValue_WhenValidDescriptionIsProvided()
    {
        //Arrange
        string validDescription = Faker.Lorem.Sentence(10);
        Result<AdoptionAnnouncementDescription> result = AdoptionAnnouncementDescription.Create(validDescription);

        //Act
        string toStringResult = result.Value.ToString();

        //Assert
        toStringResult.ShouldBe(validDescription);
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqual()
    {
        //Arrange
        string description = Faker.Lorem.Sentence(10);
        Result<AdoptionAnnouncementDescription> result1 = AdoptionAnnouncementDescription.Create(description);
        Result<AdoptionAnnouncementDescription> result2 = AdoptionAnnouncementDescription.Create(description);

        //Act & Assert
        result1.Value.ShouldBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        //Arrange
        Result<AdoptionAnnouncementDescription> result1 = AdoptionAnnouncementDescription.Create(Faker.Lorem.Sentence(10));
        Result<AdoptionAnnouncementDescription> result2 = AdoptionAnnouncementDescription.Create(Faker.Lorem.Sentence(15));

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }
}
