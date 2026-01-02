using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class CatWeightTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueProvided()
    {
        //Arrange & Act
        Result<CatWeight> result = CatWeight.Create(5500);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ValueInGrams.ShouldBe(5500);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueBelowMinimum()
    {
        //Arrange & Act
        Result<CatWeight> result = CatWeight.Create(50);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.WeightProperty.BelowMinimum(50, CatWeight.MinWeightGrams));
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueAboveMaximum()
    {
        //Arrange & Act
        Result<CatWeight> result = CatWeight.Create(25000);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.WeightProperty.AboveMaximum(25000, CatWeight.MaxWeightGrams));
    }
}
