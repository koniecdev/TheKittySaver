using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class CatNameTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueProvided()
    {
        //Arrange & Act
        Result<CatName> result = CatName.Create("Fluffy");

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Fluffy");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsEmpty()
    {
        //Arrange & Act
        Result<CatName> result = CatName.Create("   ");

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.NameProperty.NullOrEmpty);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsTooLong()
    {
        //Arrange
        string longValue = new('a', CatName.MaxLength + 1);

        //Act
        Result<CatName> result = CatName.Create(longValue);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.NameProperty.LongerThanAllowed);
    }
}
