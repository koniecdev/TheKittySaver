using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class CatDescriptionTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueProvided()
    {
        //Arrange & Act
        Result<CatDescription> result = CatDescription.Create("Valid description");

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Valid description");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsEmpty()
    {
        //Arrange & Act
        Result<CatDescription> result = CatDescription.Create("   ");

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.DescriptionProperty.NullOrEmpty);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsTooLong()
    {
        //Arrange
        string longValue = new('a', CatDescription.MaxLength + 1);

        //Act
        Result<CatDescription> result = CatDescription.Create(longValue);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.DescriptionProperty.LongerThanAllowed);
    }
}
