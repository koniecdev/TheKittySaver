using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class ListingSourceTests
{
    [Fact]
    public void Shelter_ShouldReturnSuccess_WhenValidNameProvided()
    {
        //Arrange & Act
        Result<ListingSource> result = ListingSource.Shelter("Animal Shelter");

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.SourceName.ShouldBe("Animal Shelter");
    }

    [Fact]
    public void PrivatePerson_ShouldReturnSuccess_WhenValidNameProvided()
    {
        //Arrange & Act
        Result<ListingSource> result = ListingSource.PrivatePerson("John Doe");

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.SourceName.ShouldBe("John Doe");
    }

    [Fact]
    public void Foundation_ShouldReturnSuccess_WhenValidNameProvided()
    {
        //Arrange & Act
        Result<ListingSource> result = ListingSource.Foundation("Cat Foundation");

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.SourceName.ShouldBe("Cat Foundation");
    }

    [Fact]
    public void Shelter_ShouldReturnFailure_WhenSourceNameIsEmpty()
    {
        //Arrange & Act
        Result<ListingSource> result = ListingSource.Shelter("   ");

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.ListingSourceProperty.SourceNameNullOrEmpty);
    }

    [Fact]
    public void Shelter_ShouldReturnFailure_WhenSourceNameIsTooLong()
    {
        //Arrange
        string longName = new('a', ListingSource.MaxSourceNameLength + 1);

        //Act
        Result<ListingSource> result = ListingSource.Shelter(longName);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.ListingSourceProperty.SourceNameLongerThanAllowed);
    }
}
