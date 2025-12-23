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
        Result<CatWeight> result = CatWeight.Create(5.5m);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ValueInKilograms.ShouldBe(5.5m);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueBelowMinimum()
    {
        //Arrange & Act
        Result<CatWeight> result = CatWeight.Create(0.05m);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.WeightProperty.BelowMinimum(0.05m, CatWeight.MinWeightKg));
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueAboveMaximum()
    {
        //Arrange & Act
        Result<CatWeight> result = CatWeight.Create(25m);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.WeightProperty.AboveMaximum(25m, CatWeight.MaxWeightKg));
    }
}
